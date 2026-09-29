# ECommerce —— 电商后台管理系统

基于 **.NET 10 + Vue 3** 的全栈后台管理系统。后端按**整洁架构（Clean Architecture）**思想分层，使用 Minimal API 开发；前端基于 vue3-element-admin 模板二次开发。系统涵盖 RBAC 权限、商品管理、字典配置、通知公告、操作日志等模块，支持 JWT 双令牌鉴权、图形验证码、SSE 实时推送。

## 技术栈

| | 技术 |
| --- | --- |
| 后端 | .NET 10、ASP.NET Core Minimal API、FreeSql（PostgreSQL）、FluentValidation、JWT、Swagger、ImageSharp |
| 前端 | Vue 3、TypeScript、Vite、Element Plus、Pinia、Vue Router、ECharts |
| 环境 | PostgreSQL 15、Node.js ≥ 20.19、pnpm |

## 架构设计

解决方案按整洁架构拆分为 5 个项目，**依赖严格向内，内层不感知外层技术**：

```
        Api ──► Application ──► Domain
         │           │            ▲
         └───────────┴────────────┘
                  Infrastructure
        Shared 被所有层引用（公共模型与工具）
```

| 项目 | 职责 | 引用 |
| --- | --- | --- |
| **ECommerce.Api** | 表现层：Minimal API 端点、中间件、JWT、Swagger、启动入口 | Application、Infrastructure、Shared |
| **ECommerce.Application** | 用例层：业务服务（Service）、DTO、FluentValidation 校验器、JWT/验证码/SSE 业务逻辑 | Domain、Shared |
| **ECommerce.Domain** | 领域核心：实体（15 个）与仓储**接口**，只定义"要什么" | 无（纯净模型） |
| **ECommerce.Infrastructure** | 技术实现：FreeSql 仓储实现、CodeFirst 建表 | Domain、Shared |
| **ECommerce.Shared** | 公共工具箱：ApiResult、业务异常、状态码、密码哈希 | 无 |

**关键设计**：

- **依赖倒置**：Application/Domain 只持有仓储接口（如 `ISysUserRepository`），Infrastructure 负责实现并在 Api 层的 DI 容器中装配，业务代码不引用 FreeSql
- **单向依赖**：Infrastructure 只依赖 Domain，不依赖 Application；切换数据库只改 Infrastructure

## 目录结构

```
ECommerce.Api/
├── Endpoints/          # Minimal API 端点（具名静态方法，无 Controller）
│   ├── ApiEndpoints.cs     # 端点总注册，统一前缀 /api/v1
│   ├── AuthEndpoints.cs    # 验证码、登录、刷新令牌、退出
│   ├── SysManageEndpoints.cs # 用户/角色/菜单/部门/字典/通知/日志/配置
│   ├── ProductEndpoints.cs # 商品demo CRUD
│   ├── UploadEndpoints.cs  # 图片上传
│   └── SseEndpoints.cs     # SSE 长连接
├── Middleware/         # 全局异常、操作日志中间件
├── Filters/            # ValidationEndpointFilter（自动参数校验）
└── Program.cs          # 服务注册 + 中间件管道总装

ECommerce.Application/Features/
├── SystemManage/       # SysAuthService、SysRbacServices、SysUserService、
│                       # SysMiscServices、SysJwtService、SysCaptchaService、Dtos、Validators
└── Product/            # ProductService

ECommerce.Domain/
├── Entities/           # Product/（Products、Categories）、System/（13 个 RBAC 实体）
└── Repositories/       # 全部为接口

ECommerce.Infrastructure/Persistence/
├── Repositories/       # 仓储接口的 FreeSql 实现
└── DbSeeder.cs         # CodeFirst 自动建表
```

## 快速启动

### 1. 准备 PostgreSQL

```bash
docker run -d --name pg_container -p 5432:5432 \
  -e POSTGRES_PASSWORD=postgres postgres:15
```

开发使用的数据库名为 `mesmini`（连接串见 `ECommerce.Api/appsettings.json`）。应用启动时 FreeSql CodeFirst 会**自动创建全部表结构**；账号等业务数据沿用已有库数据，默认超管账号 `root / 123456`。

### 2. 启动后端

```bash
cd ECommerce.Api
dotnet run
```

- 接口服务：http://localhost:5080
- Swagger（仅开发环境）：http://localhost:5080/swagger

### 3. 启动前端

```bash
cd vue3-element-admin
pnpm install
pnpm dev
```

- 前端页面：http://localhost:3000
- Vite 已配置代理：`/dev-api` → `http://localhost:5080`（见 `.env.development`），Mock 服务默认关闭

## 核心功能

- **认证鉴权**：算术图形验证码（ImageSharp 生成 PNG，答案内存缓存 5 分钟）；JWT 双令牌——access token 120 分钟、refresh token 7 天；401/403 统一返回 ApiResult
- **RBAC 权限**：用户-角色-菜单-部门四维模型，角色菜单分配、数据权限、后端动态路由树下发，前端据此生成菜单与按钮级权限
- **系统管理**：用户、角色、菜单、部门、字典（含字典项）、系统配置、通知公告、操作日志
- **商品管理**：分类、分页查询、SKU 唯一校验、上下架状态
- **实时推送**：SSE 长连接（`/api/v1/sse/connect`），按用户定向推送，15 秒心跳保活
- **文件上传**：图片上传至 wwwroot，静态文件中间件直接访问
- **工程化**：统一 ApiResult 返回、全局异常中间件、写操作审计日志中间件、FluentValidation 端点过滤器自动校验

## 一个请求的链路

以「登录」为例，请求依次经过：

```
POST /api/v1/auth/login
  → ExceptionHandlingMiddleware   未捕获异常统一转 ApiResult
  → OperationLogMiddleware       写操作审计（记录操作人/IP/耗时/状态）
  → UseAuthentication / UseAuthorization
  → ValidationEndpointFilter     先执行 FluentValidation 校验
  → AuthEndpoints.Login          校验验证码 → SysAuthService 校验密码、签发双令牌
  → SysUserRepository（Domain 接口 → Infrastructure 的 FreeSql 实现）
  → PostgreSQL
```

端点处理器只做"收参 → 调 Service → 包 ApiResult"；业务判断在 Service；SQL 在 Repository。

## 主要接口

| 模块 | 方法 & 路径 |
| --- | --- |
| 认证 | `GET /auth/captcha`、`POST /auth/login`、`POST /auth/refresh-token`、`DELETE /auth/logout` |
| 用户 | `/users`（分页/增删改查）、`/users/me`、`/users/me/routes`、`/users/{id}/password/reset` |
| 角色/菜单/部门 | `/roles`、`/menus`、`/depts`（树形 CRUD） |
| 字典 | `/dicts`、`/dicts/{dictCode}/items` |
| 通知/配置/日志 | `/notices`、`/configs`、`/logs` |
| 商品 | `GET/POST /products`、`PUT/DELETE /products/{id}`、`/products/meta/categories` |
| 其他 | `POST /upload/image`、`GET /sse/connect` |

所有接口统一前缀 `/api/v1`，统一返回格式：

```json
{ "code": "0", "success": true, "msg": "ok", "data": {} }
```

## 配置说明

`ECommerce.Api/appsettings.json`：

```jsonc
{
  "ConnectionStrings": {
    "Default": "Host=localhost;Port=5432;Username=postgres;Password=postgres;Database=mesmini"
  },
  "Jwt": {
    "Key": "ECommerce-Super-Secret-Key-Change-Me-In-Production-...",
    "ExpireMinutes": 120
  }
}
```

> JWT `Key` 为演示用密钥，生产环境务必替换为长随机串并通过环境变量注入。
