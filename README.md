# Staff Hub System

Staff Hub System is a multi-project ASP.NET Core MVC solution that is organized into presentation, application, and data layers.

## Solution Layout

- `Presentation/` — ASP.NET Core MVC web application (startup project).
- `Application/` — application services and DTOs.
- `Data/` — Entity Framework Core context, models, migrations, and seed data.
- `StaffHubSystem.sln` / `EmployeeAppReloaded.sln` — solution files.

## Prerequisites

- .NET 9 SDK
- SQL Server (local or remote) for the EF Core database

## Configuration

Update the connection string and service settings in `Presentation/appsettings.json` (or `Presentation/appsettings.Development.json`):

- `ConnectionStrings:DefaultConnection`
- `CloudinarySettings`
- `MailSettings`

## Build

```bash
dotnet restore

dotnet build StaffHubSystem.sln
```

## Database Setup

Apply migrations using the Presentation project as the startup project:

```bash
dotnet ef database update \
  --project Data/Data.csproj \
  --startup-project Presentation/Presentation.csproj
```

## Run

```bash
dotnet run --project Presentation/Presentation.csproj
```

Then open the app at `https://localhost:5001` or `http://localhost:5000` (depending on your launch settings).

## Notes

- Seed data is applied automatically in development mode on app startup.
- ASP.NET Core Identity is configured with strong password requirements.
