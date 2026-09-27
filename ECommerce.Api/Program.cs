using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using ECommerce.Api.Authentication;
using ECommerce.Api.Endpoints;
using ECommerce.Api.Middleware;
using ECommerce.Application;
using ECommerce.Infrastructure;
using ECommerce.Shared.Results;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

// ============================================================================
// Program.cs：整个应用的“总装车间”。
// 职责只有两件事：
//   1) 注册服务（告诉 DI 容器：谁实现谁）——往上半段，builder.Services.AddXxx
//   2) 组装请求管道（每个请求进来依次经过哪些中间件）——往下半段，app.UseXxx
// 其余业务代码都不在这里，这里只做“接线”。
// ============================================================================

var builder = WebApplication.CreateBuilder(args);

// ----------------------------------------------------------------------------
// 1. Minimal API 与 JSON 序列化
// ----------------------------------------------------------------------------
// 本项目使用 Minimal API（端点处理器见 Endpoints/ 目录，全部为具名静态方法），
// 不再注册 MVC 控制器。自动校验通过 ValidationEndpointFilter（IEndpointFilter）实现。
builder.Services.Configure<JsonOptions>(options =>
{
    // 让中文等非 ASCII 字符原样输出，而不是 \uXXXX 转义，方便看返回。
    // Minimal API 的端点 JSON 绑定与 TypedResults/Results 统一使用该配置。
    options.SerializerOptions.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
});
builder.Services.AddEndpointsApiExplorer(); // 为 Swagger 提供端点元数据
builder.Services.AddHttpContextAccessor(); // 允许在任意服务里拿到当前请求（JWT 等会用到）

// ----------------------------------------------------------------------------
// 2. JWT 鉴权
// ----------------------------------------------------------------------------
// 读取 appsettings.json 的 "Jwt" 节，生成签名密钥（SysJwtService 和 JwtBearer 中间件共用）。
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(JwtSettings.SectionName));
var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>() ?? new JwtSettings();
// 用配置里的密钥生成对称签名密钥，签发和校验都用它。
var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key));

// 注册 JwtBearer 方案：[Authorize] 会从请求头 Authorization: Bearer <token> 读令牌并校验。
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false; // 本地 http 演示，不强制 https
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true, ValidIssuer = jwtSettings.Issuer, // 校验签发者
            ValidateAudience = true, ValidAudience = jwtSettings.Audience, // 校验受众
            ValidateIssuerSigningKey = true, IssuerSigningKey = signingKey, // 校验签名密钥
            ValidateLifetime = true, ClockSkew = TimeSpan.FromSeconds(30), // 校验有效期，时钟偏差留 30 秒
            NameClaimType = ClaimTypes.Name, // 把 JWT 里的用户名声明映射成 .NET 的 Name
            RoleClaimType = ClaimTypes.Role // 把角色声明映射成 .NET 的 Role（供 [Authorize(Roles=...)] 使用）
        };

        // Token 校验失败时返回统一 ApiResult 格式（code=A0230），前端据此触发 refresh token
        options.Events = new JwtBearerEvents
        {
            OnChallenge = async context =>
            {
                context.HandleResponse();
                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json;charset=utf-8";
                var result = new ApiResult
                {
                    Code = ResultCodes.AccessTokenInvalid,
                    Success = false,
                    Msg = "访问令牌无效或过期"
                };
                await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(result));
            },
            OnForbidden = async context =>
            {
                context.Response.StatusCode = 403;
                context.Response.ContentType = "application/json;charset=utf-8";
                var result = new ApiResult
                {
                    Code = ResultCodes.PermissionDenied,
                    Success = false,
                    Msg = "权限不足"
                };
                await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(result));
            }
        };
    });
builder.Services.AddAuthorization(); // 启用授权策略（[Authorize]、[Authorize(Roles=...)]）

// ----------------------------------------------------------------------------
// 3. 各业务层的服务注册
// ----------------------------------------------------------------------------
builder.Services.AddApplication(); // → Application 内部注册 ProductService、Sys 各服务与所有校验器
builder.Services.AddInfrastructure(builder.Configuration); // → 内部注册 FreeSql、仓储、缓存、种子

// ----------------------------------------------------------------------------
// 4. Swagger / Swagger UI（接口文档 + 在线调试）
// ----------------------------------------------------------------------------
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ECommerce API",
        Version = "v1",
        Description = "DDD 五层架构（Api / Application / Domain / Infrastructure / Shared）示例电商接口"
    });

    // 定义一个名为 "Bearer" 的安全方案，让 Swagger UI 右上角出现 Authorize 按钮。
    var bearerScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "请输入 JWT，格式：Bearer {token}"
    };
    options.AddSecurityDefinition("Bearer", bearerScheme);

    // 把该安全方案设为全局默认要求（受保护接口在文档里会显示小锁）。
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        { new OpenApiSecuritySchemeReference("Bearer", document), new List<string>() }
    });
});

// 以上全部是“注册服务”，builder.Build() 之后才开始“组装管道”。
var app = builder.Build();

// ----------------------------------------------------------------------------
// 5. 请求管道（注意顺序很重要！）
// ----------------------------------------------------------------------------

// 5.1 最外层：兜底全局异常。放最前面，后面任何环节抛异常都能被它接住并转成统一 ApiResult。
app.UseMiddleware<ExceptionHandlingMiddleware>();

// 5.2 操作日志：记录写操作（POST/PUT/DELETE）到 sys_log 表
app.UseMiddleware<OperationLogMiddleware>();

// 5.2 仅开发环境开启 Swagger 页面与文档。
if (app.Environment.IsDevelopment())
{
    app.UseSwagger(); // 输出 swagger.json
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ECommerce API v1")); // 可视化页面
}

// 5.3 启动时自动建表并写入种子数据（admin/demo、6 条示例商品）。
app.UseDatabaseInitializer();

// 5.3.1 静态文件：允许访问 wwwroot 下的文件（上传的图片等）
app.UseStaticFiles();

// 5.4 认证 → 授权 → 路由端点。必须先 UseAuthentication 再 UseAuthorization。
app.UseAuthentication(); // 解析并校验 token，把登录信息放进 User
app.UseAuthorization(); // 检查 RequireAuthorization() 的端点是否放行
app.MapApiEndpoints(); // 注册全部 Minimal API 端点（/api/v1/...）

app.Run(); // 启动 Kestrel，开始监听端口