# ECommerce —— .NET 10 五层分层架构示例（带详细导航）

这是一个**能直接跑起来**的电商后端接口，目的是演示一套干净的分层结构：一个 HTTP 请求进来，依次经过表现层 → 业务层 → 领域层 → 基础设施 → 数据库，再原路返回。全程已接入 JWT 登录鉴权、Swagger 文档、统一返回格式、参数校验、缓存、自动建表和种子数据。

> 如果你刚看完代码有点懵，**直接看第三节「一个请求的完整旅程」**，看完整个调用链就清楚了。

---

## 一、最快跑起来（3 步）

**1. 准备 PostgreSQL**（本机已有 `pg_container` 的话跳过）：

```bash
docker run -d --name pg_container -p 5432:5432 \
  -e POSTGRES_PASSWORD=postgres postgres:15
docker exec pg_container psql -U postgres -c "CREATE DATABASE ecommerce;"
```

**2. 启动**：

```bash
cd ECommerce.Api
dotnet run
```

**3. 打开 Swagger 调试页面**：http://localhost:5080/swagger

首次启动会自动建表并写入种子数据。

| 种子账号 | 密码 | 角色 | 能干什么 |
| --- | --- | --- | --- |
| `admin` | `123456` | Admin | 增删改查商品 |
| `demo` | `123456` | User | 查询、创建商品（改/删返回 403） |

---

## 二、五个项目分别是什么

| 项目 | 一句话定位 | 能不能依赖别人 |
| --- | --- | --- |
| **ECommerce.Api** | 最外层，接 HTTP 请求：Controller、Swagger、JWT、中间件、启动入口 | 可依赖 Application、Infrastructure、Shared |
| **ECommerce.Application** | 业务用例层：编排业务逻辑（Service）、定义入参出参（DTO）、参数校验 | 依赖 Domain、Shared；**不认识**数据库 |
| **ECommerce.Domain** | 纯业务核心：实体、领域事件、仓储**接口**（只定义“要什么”，不管怎么实现） | **零依赖**，纯净的业务模型 |
| **ECommerce.Infrastructure** | 技术实现层：真正连 PostgreSQL（FreeSql）、实现仓储接口、Redis/内存缓存、建表 | 依赖 Domain、Application（实现它们定义的接口） |
| **ECommerce.Shared** | 所有人都能用的工具箱：统一返回 ApiResult、状态码、业务异常、加密工具 | **零依赖** |

依赖箭头（只能从上往下、或 Infrastructure 指向它要实现的接口）：

```
        Api ──► Application ──► Domain  （业务层只看到接口和实体，不知道数据库）
         │           ▲            ▲
         └───────────┴────────────┘
              Infrastructure 实现 Domain / Application 里的接口
              Shared 被所有人引用（公共模型与工具）
```

**关键设计**：业务层（Application/Domain）里**只有接口，没有 FreeSql**。真正写 SQL 的代码全在 Infrastructure。这样把“业务规则”和“用什么数据库”解耦——以后换 MySQL，业务代码一行都不用改。

---

## 三、一个请求的完整旅程（以「创建商品」为例）

```
浏览器/Postman
   │  POST /api/products  (Header: Authorization: Bearer <token>)
   ▼
[1] ExceptionHandlingMiddleware   全局异常兜底（最先注册，任何错误都在这里转成统一返回）
   ▼
[2] UseAuthentication            JWT 校验 token，识别出当前用户
   ▼
[3] UseAuthorization             检查 [Authorize]（这里要求必须登录）
   ▼
[4] ProductsController.Create()   接收 JSON，校验通过后调用 Service
   ▼
[5] ProductService.CreateAsync()  业务编排：
      ├─ FluentValidation 校验参数（名称、价格、库存）
      ├─ AnyBySkuAsync 检查编码是否重复
      ├─ new Product 实体 → RaiseDomainEvent 领域事件
      ├─ 调仓储接口 _productRepository.InsertAsync()
      ▼
[6] ProductRepository             （Infrastructure 层）把接口翻译成 FreeSql 的 SQL
      ▼
[7] PostgreSQL                    INSERT INTO "Product" ... RETURNING "Id"
      ▼
原路返回：仓储回填 Id → Service 组装 ProductDto → Controller 包成 ApiResult → JSON 返回
```

**记忆点**：Controller 只做“接参数、调 Service、包返回”；真正的业务判断全在 Service；数据库操作全在 Repository。这就是分层的意义。

---

## 四、逐文件夹、逐文件讲解

### 1. ECommerce.Shared —— 公共工具箱（零依赖）

| 文件 | 作用 |
| --- | --- |
| `Results/ApiResult.cs` | **统一返回模型**。所有接口都长这样：`{ code, success, message, data, timestamp }`。`code=0` 成功 |
| `Results/PagedResult.cs` | 分页结果（items、total、pageIndex、pageSize、totalPages） |
| `Results/ResultCodes.cs` | 业务状态码常量（400 校验错 / 401 未登录 / 403 无权限 / 404 不存在 / 409 冲突…） |
| `Constants/AppConstants.cs` | 全局常量：角色名、缓存键前缀 |
| `Exceptions/BusinessException.cs` | 业务异常。Service 里 `throw BusinessException.NotFound(...)`，中间件接住转成统一错误返回 |
| `Utils/PasswordHasher.cs` | 密码加密/校验（PBKDF2，纯 .NET 实现，不存明文） |
| `Utils/JsonHelper.cs` | JSON 序列化小工具（缓存序列化用） |

### 2. ECommerce.Domain —— 纯业务核心（零依赖）

| 文件 | 作用 |
| --- | --- |
| `Common/BaseEntity.cs` | 所有实体的基类：主键 `Id`、创建/更新时间、一个领域事件队列 |
| `Entities/Product.cs` | 商品实体（名称、编码、价格、库存、是否上架） |
| `Entities/User.cs` | 用户实体（用户名、密码哈希、角色、是否启用） |
| `Events/IDomainEvent.cs` | 领域事件标记接口 |
| `Events/ProductCreatedEvent.cs` | “商品已创建”事件（DDD 里用事件解耦，当前演示为记录日志） |
| `Repositories/IProductRepository.cs` | 商品仓储**接口**：只声明业务要用的方法，不写实现 |
| `Repositories/IUserRepository.cs` | 用户仓储接口：按用户名查、是否重名、插入 |

> 这里**只有接口没有 SQL**。“我需要按 SKU 查商品”是业务需求，写在 Domain；具体怎么查（FreeSql）写在 Infrastructure。

### 3. ECommerce.Application —— 业务用例层

| 文件/目录 | 作用 |
| --- | --- |
| `Common/Interfaces/ICacheService.cs` | 缓存抽象接口（Redis 还是内存由 Infrastructure 决定，业务层只依赖这个抽象） |
| `Common/Interfaces/IJwtTokenGenerator.cs` | 生成 JWT 的抽象接口（实现放在 Api，业务层不碰 JWT 细节） |
| `Common/Models/PagedQuery.cs` | 分页查询入参（页码、每页条数、关键字） |
| `DependencyInjection.cs` | 把本层的 Service、校验器注册进 DI 容器（`AddApplication()`） |
| `Features/Auth/` | 登录注册用例：`LoginDto/RegisterDto`（入参）、`AuthResultDto`（出参）、`*Validator.cs`（规则）、`IAuthService` + `AuthService` |
| `Features/Products/` | 商品用例：`CreateProductDto/UpdateProductDto/ProductDto`、`ProductMapper`（实体→DTO）、`*Validator.cs`、`IProductService` + `ProductService` |

`Features/` 按**业务功能**分目录（Auth、Products），而不是按技术类型乱堆——找“登录相关的一切”直接进 `Features/Auth/`。

### 4. ECommerce.Infrastructure —— 技术实现层

| 文件/目录 | 作用 |
| --- | --- |
| `Persistence/Repositories/ProductRepository.cs` | 商品仓储实现：把 Domain 的接口翻译成 FreeSql 查询，直接操作数据库，无继承基类 |
| `Persistence/Repositories/UserRepository.cs` | 用户仓储实现 |
| `Persistence/DbSeeder.cs` | 启动时自动建表（CodeFirst）并写入种子用户/商品 |
| `Caching/RedisCacheService.cs` | Redis 缓存实现 |
| `Caching/MemoryCacheService.cs` | 本地内存缓存实现（没装 Redis 时自动用它） |
| `Options/RedisOptions.cs` | Redis 配置选项 |
| `DependencyInjection.cs` | 组装 FreeSql、注册仓储、按开关选缓存、`UseDatabaseInitializer()` |

### 5. ECommerce.Api —— 表现层（接 HTTP）

| 文件/目录 | 作用 |
| --- | --- |
| `Program.cs` | **总装入口**：注册服务、配置 JWT/Swagger、拼装请求中间件管道（已加详细注释） |
| `Controllers/AuthController.cs` | 登录/注册接口 |
| `Controllers/ProductsController.cs` | 商品增删改查接口，用 `[Authorize]`、`[Authorize(Roles=Admin)]` 控制权限 |
| `Authentication/JwtSettings.cs` | JWT 配置（签发者、密钥、过期时间） |
| `Authentication/JwtTokenGenerator.cs` | 实现 Application 的 IJwtTokenGenerator，真正生成 token |
| `Middleware/ExceptionHandlingMiddleware.cs` | 全局异常中间件：任何未捕获异常都转成统一 `ApiResult` 返回 |
| `appsettings.json` | 数据库连接串、JWT、Redis 等配置 |

---

## 五、接口与统一返回

| 方法 | 路由 | 谁能访问 | 说明 |
| --- | --- | --- | --- |
| POST | `/api/auth/register` | 匿名 | 注册并返回 JWT |
| POST | `/api/auth/login` | 匿名 | 登录并返回 JWT |
| GET | `/api/products` | 匿名 | 分页+关键字：`?pageIndex=1&pageSize=10&keyword=键盘` |
| GET | `/api/products/{id}` | 匿名 | 商品详情 |
| POST | `/api/products` | 登录用户 | 创建商品 |
| PUT | `/api/products/{id}` | 仅 Admin | 更新商品 |
| DELETE | `/api/products/{id}` | 仅 Admin | 删除商品 |

统一返回格式（成功/失败都一样）：

```json
{ "code": 0, "success": true, "message": "ok", "data": {}, "timestamp": 1789607960689 }
```

失败时：`code` 为错误码，`success=false`，`message` 是人能看懂的原因（如「商品编码 SKU-1001 已存在」）。

快速联调：

```bash
# 登录拿 token
TOKEN=$(curl -s -X POST http://localhost:5080/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"userName":"admin","password":"123456"}' | python3 -c "import sys,json;print(json.load(sys.stdin)['data']['accessToken'])")

# 带 token 创建商品
curl -s -X POST http://localhost:5080/api/products \
  -H "Content-Type: application/json" -H "Authorization: Bearer $TOKEN" \
  -d '{"name":"测试商品","sku":"SKU-1001","price":99.9,"stock":10}'
```

---

## 六、配置说明（`ECommerce.Api/appsettings.json`）

```jsonc
{
  "ConnectionStrings": {
    "Default": "Host=localhost;Port=5432;Username=postgres;Password=postgres;Database=ecommerce"
  },
  "Jwt": { "Issuer": "...", "Audience": "...", "Key": "改我！", "ExpireMinutes": 120 },
  "Redis": { "Enabled": false, "ConnectionString": "127.0.0.1:6379" }
}
```

- 数据库：改连接串指向你的 PG，空库即可，启动自动建表。
- JWT：`Key` 是签名密钥，**正式环境务必改成长随机串并放到环境变量/密钥管理**。
- Redis：`Enabled=false` 用内存缓存（开箱即用）；改成 `true` 并填 Redis 地址即切换为分布式缓存。商品详情/列表缓存 30 秒，增删改自动清缓存。

---

## 七、常见问题

- **打开是 404**：确认访问的是 `http://localhost:5080/swagger`（不是根路径 `/`）。
- **连不上数据库**：检查 PG 是否启动、`appsettings.json` 连接串和密码是否正确。
- **401 / 403**：401 是没带/带错 token；403 是登录了但角色不够（改/删商品需要 Admin）。
- **表已存在要重置**：`docker exec pg_container psql -U postgres -d ecommerce -c 'TRUNCATE "User","Product" RESTART IDENTITY CASCADE;'` 后重启，会重新写入种子。
