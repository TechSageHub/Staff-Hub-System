# AGENTS.md — StaffHub / EmployeeAppReloaded

.NET 9 ASP.NET Core MVC, 3 projects. No tests, no lint/format config, `.github/workflows/` is empty.

## Layout

- `Presentation/` — startup project (MVC controllers, views, `Program.cs`). References both others.
- `Application/` — services (`Services/<Feature>/I<Feature>Service.cs`), DTOs (`Dtos/`), `ApplicationServiceExtension.AddServices()`.
- `Data/` — `Context/EmployeeAppDbContext` (extends `IdentityDbContext<IdentityUser>`), `Model/`, `Configurations/` (auto-applied via `ApplyConfigurationsFromAssembly`), `Migrations/`, `SeedData.cs`.
- `StaffHubSystem.sln` and `EmployeeAppReloaded.sln` are identical; use `StaffHubSystem.sln` (per README).

## Commands

```bash
dotnet restore
dotnet build StaffHubSystem.sln
dotnet run --project Presentation/Presentation.csproj   # http://localhost:5097, https://localhost:7271 (launchSettings; README's 5000/5001 is stale)
dotnet ef database update --project Data/Data.csproj --startup-project Presentation/Presentation.csproj
```

Docker (preferred for full stack):

```bash
cp .env.example .env   # then set strong SA_PASSWORD
docker compose up --build   # web on ${APP_PORT:-8080}, sqlserver on ${SQL_PORT:-1433}
docker compose down         # add -v to wipe sqlserver-data volume
```

There is no test suite — verify with `dotnet build StaffHubSystem.sln` (+ `dotnet ef` command above when touching models).
New migration when touching `Data/Model/` or `Data/Configurations/`:

```bash
dotnet ef migrations add <Name> --project Data/Data.csproj --startup-project Presentation/Presentation.csproj
```

## DB / startup behavior (`Presentation/Program.cs`)

- App runs `MigrateAsync()` on every startup — manual `database update` is only needed for local non-Docker runs or to verify a new migration.
- `SeedData.Initialize` runs **only** when `ASPNETCORE_ENVIRONMENT=Development`: creates `Admin`/`User` roles + 5 onboarding modules (idempotent).
- Admin user `admin@staffhub.com` is created **only** if `ADMIN_INITIAL_PASSWORD` is set (launchSettings sets it to a dev value; compose does not — set it explicitly if you need the admin in Docker).
- SQL Server only. Local default: `Server=localhost;Database=EmployeeDbReloaded;Trusted_Connection=True;TrustServerCertificate=True;`. Compose overrides via `ConnectionStrings__DefaultConnection` pointing at host `sqlserver`. Render needs an external SQL Server (no managed SQL Server there).

## Config / secrets

- Never commit secrets to tracked `appsettings.json` / `appsettings.Development.json` (both contain empty Cloudinary/Mail placeholders). Use `.env` (see `.env.example`), env vars with `__` nesting (e.g. `ConnectionStrings__DefaultConnection`), or user secrets (`UserSecretsId` is configured on Presentation).
- Docker port binding is via `APP_PORT`/`SQL_PORT`; Render binding is via `Presentation/start.sh` reading `$PORT` into `ASPNETCORE_URLS` — don't hardcode ports.

## MVC / code conventions

- Global `AutoValidateAntiforgeryTokenAttribute` — every POST form needs an antiforgery token.
- Global `OnboardingCompletionFilter` (registered via `AddService`): non-admin authenticated users with incomplete onboarding are redirected to `Onboarding/Index`; `Account` and `Onboarding` controllers are exempt. New controllers inherit this — don't work around it without reason.
- DI: register new `Application` services in `ApplicationServiceExtension.AddServices()` as `Scoped`. The 3 exceptions registered directly in `Program.cs` are `IAddressService`, `IStateService`, `IImageService`.
- Mapping is manual extension methods: `Presentation/DtoMapping/Mapperly.cs` (ViewModel↔DTO) plus `Application/ContractMapping/`. Extend those; don't add AutoMapper.
- Identity rules are strict: 8+ chars, upper+lower+digit+symbol, unique email; lockout 3 failures / 5 min; cookie 10 days, `SlidingExpiration=false`.
- Keep `QuestPDF.Settings.License = ...Community` in `Program.cs`.
- External integrations: Cloudinary (`IImageService`), SendGrid (`IEmailService`), CsvHelper, ToastNotification (Notyf TopRight, 5s).

## Build gotcha

- `Presentation.csproj` has a Windows-only pre-build target that `taskkill /F /IM Presentation.exe`. Docker builds already pass `/p:KillLockedPresentationProcessBeforeBuild=false` — keep that flag on any new docker/CI build invocation.
- Only real Dockerfile is `Presentation/Dockerfile` (repo-root `Dockerfile.txt` is empty — ignore it).
