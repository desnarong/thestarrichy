# AGENTS.md — TheStarRichy

## Project Overview

This is a .NET 7 MLM (Multi-Level Marketing) platform with two projects:

| Project | Type | Role |
|---------|------|------|
| `TheStarRichyAPI` | ASP.NET Core Web API | Backend API — direct DB access, JWT auth, KBank payments |
| `TheStarRichyProject` | ASP.NET Core MVC | Frontend web app — calls the API via RestSharp, cookie auth |

Both target `net7.0` with nullable enabled.

---

## Build & Run Commands

```bash
# Build entire solution
dotnet build TheStarRichyProject.sln

# Run API (default: https://localhost:7259)
dotnet run --project TheStarRichyAPI/TheStarRichyApi.csproj

# Run MVC app (default: https://localhost:4527)
dotnet run --project TheStarRichyProject/TheStarRichyProject.csproj
```

There are **no test projects, no CI/CD workflows** (`.github/workflows/` is empty), and no Makefile or Docker config.

---

## Database

- **SQL Server** — connection via raw `System.Data.SqlClient` (no Entity Framework in the API)
- API reads from `ConnectionStrings:MLMConnectionString` in `appsettings.json`
- MVC uses `DbConnFactory` (singleton, Dapper-based) reading from `DbConnectionString` config key — **this key is NOT in the committed `appsettings.json`**; it must be supplied via environment or secrets
- Stored procedures are heavily used (e.g., `sp_SyncMemberToM06`, `[comm].[sp_sec_open_key]`)
- Dapper `CommandTimeout` is set to 300 seconds
- Encrypted columns exist — `DbConnFactory.CreateCryptConnection()` opens a key before use

---

## Architecture & Data Flow

```
Browser → TheStarRichyProject (MVC) → RestSharp HTTP → TheStarRichyAPI → SqlClient → SQL Server
                                           ↕
                              TheStarRichyProject also has direct DB access
                              via DbConnFactory + Dapper (for legacy queries)
```

### API Project (`TheStarRichyAPI`)

- **Controllers** in `Controllers/` — thin, delegate to services, return `IActionResult`
- **Services** in `Services/` — contain all business logic and raw SQL
  - Every service has a matching interface (e.g., `ILoginService` / `LoginService`)
  - All registered as scoped in `Program.cs`
  - Services take `IConfiguration` via constructor injection to read connection strings
  - KBank services (`KbankAuthService`, `KbankQrPaymentService`) use `HttpClientFactory` (registered via `AddHttpClient`)
- **Models** in `Models/` — DTOs/request/response classes, no EF entity classes
  - KBank models in `Models/Kbank/`
  - `KbankSettings` is bound from `appsettings.json` `Kbank` section via `IOptions<KbankSettings>`
- **Auth**: JWT Bearer tokens, validated against `Jwt:Key/Issuer/Audience` config
- **API passkey**: Every controller checks `X-Passkey` header against the value in `Api:Passkey` config (the MVC app sends this)
- **Swagger** available at `/swagger` when running
- Compression (Gzip + Brotli) and CORS (allow all origins) enabled

### MVC Project (`TheStarRichyProject`)

- **Controllers** in `Controllers/` — all inherit from `BaseController`
- `BaseController` has `[SessionCheck]` action filter that redirects to `/Auth/Login` if no `UserSession` cookie
  - Skip list: `AuthController`, `ExternalRegistrationController`, `CultureController`, `/home/GetSlideImages`, `/home/GetPopupSlideImages`
- **Services** in `Services/` — API clients that call the backend API via **RestSharp**
  - `ApiService` — generic GET/POST/DELETE, adds `Authorization: Bearer {token}` from `UserSession` cookie and `X-Passkey` header
  - `KbankApiClient`, `ProductApiClient`, `CartApiService`, `OrderApiService`, `BranchStockApiService`, `DocumentDownloadApiService`
  - All registered as scoped in `Program.cs`
- **Session/Auth**: Cookie-based — JWT token stored in `UserSession` cookie; member info in separate cookies
- **CookieHelper** (`Helper/CookieHelper.cs`): static class with cookie key constants and extension methods on `IHttpContextAccessor`
- **Middleware**: `GlobalExceptionHandlingMiddleware` — catches unhandled exceptions, returns JSON for AJAX, redirects to login for page requests
- **Localization**: 5 cultures (en-US, th-TH, lo-LA, km-KH, my-MM), resource files in `Resources/`
- Default route: `{controller=Auth}/{action=Login}`
- `CommonConfig` singleton with `Initialize()` must be called at startup (provides `DbConnectionString` to `DbConnFactory`)

---

## Naming Conventions & Patterns

- Controller route: `[Route("[controller]")]` (maps to `/ControllerName/action`)
- API endpoints use lowercase in route attributes: `[HttpGet("productgroup")]`
- Service interfaces prefixed with `I`: `ILoginService`, `IMemberService`, etc.
- Response models typically have `Success`, `Message`, plus data fields
- Models use `string.Empty` defaults, not `null`
- C# naming: PascalCase for public members, camelCase for parameters
- JSON property names expected by the API are **camelCase** (the API uses `System.Text.Json` default)

---

## Gotchas & Non-Obvious Patterns

1. **`Defualt` misspelling**: The config key is `Defualt:HourExpires` (not "Default"). Don't fix this — it's referenced throughout the codebase.

2. **Custom password encoding**: `LoginService.Encode()` uses a Caesar cipher (+3 per character) wrapped in `123{char}50{encoded}{char}50!!!!`. This is NOT BCrypt or standard hashing — it's a legacy scheme.

3. **Passkey dual check**: Login validates against TWO passkeys (`Passkey1` and `Passkey2` columns from the `S02` table). The API controller accepts either.

4. **SSL certificate bypass**: Everywhere — `ServerCertificateCustomValidationCallback = (m, c, ch, e) => true`. This is for dev/test only; don't replicate in new code without understanding implications.

5. **MVC has TWO DB access paths**: Direct via `DbConnFactory` (Dapper) AND indirect via the API (RestSharp). The MVC project can bypass its own API. New features should go through the API pattern, not direct DB.

6. **`CommonConfig.Initialize()`**: The MVC project's `Program.cs` does NOT explicitly call `CommonConfig.Initialize()`. It relies on `DbConnFactory.Instance` to trigger it on first access. If you add early startup DB calls, you may need to initialize explicitly.

7. **No Entity Framework**: Both projects use raw ADO.NET — no DbContext, no migrations. SQL is inline strings or stored procedures. Don't add EF unless explicitly asked.

8. **Session timeout**: Set in `appsettings.json` as `IdleTimeout` (minutes, default 30). Cookie expiration for `UserSession` uses `Defualt:HourExpires` (hours, default 1).

9. **KBank integration**: The KBank payment flow uses `TestMode: true` in config. It communicates with KBank's sandbox API. Webhook callbacks come to `POST /qr/payment-callback`.

10. **`Imagespath` config**: Points to `D:\therichteam.com\Images` — this is an absolute Windows path for image storage, not relative to the project.

11. **Error handling**: API catches exceptions and returns 500 with `{ message: "Internal server error" }`. MVC middleware catches unhandled and redirects to login.

12. **The API `ProductController` does NOT have `[Authorize]`** — it's a public endpoint, unlike `MemberController` which requires authorization.
