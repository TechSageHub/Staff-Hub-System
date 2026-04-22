# Staff Hub System

Staff Hub System is a multi-project ASP.NET Core MVC solution organized into presentation, application, and data layers.

## Solution Layout

- `Presentation/` - ASP.NET Core MVC web application (startup project)
- `Application/` - application services and DTOs
- `Data/` - Entity Framework Core context, models, migrations, and seed data
- `StaffHubSystem.sln` / `EmployeeAppReloaded.sln` - solution files

## Prerequisites

For local development without Docker:

- .NET 9 SDK
- SQL Server (local or remote)

For containerized development:

- Docker Desktop with Compose support

## Configuration

The app reads configuration from `Presentation/appsettings.json`, environment-specific appsettings files, and environment variables.

Important settings:

- `ConnectionStrings:DefaultConnection`
- `CloudinarySettings:CloudName`
- `CloudinarySettings:ApiKey`
- `CloudinarySettings:ApiSecret`
- `MailSettings:FromEmail`
- `MailSettings:Password`
- `MailSettings:SmtpHost`
- `MailSettings:SmtpPort`

## Run With Docker

1. Copy `.env.example` to `.env`
2. Set a strong `SA_PASSWORD` value and fill in any Cloudinary or mail settings you need
3. Start the stack:

```bash
docker compose up --build
```

The MVC app will be available at `http://localhost:8080` by default, and SQL Server will be exposed on `localhost:1433`.

Notes:

- The app runs EF Core migrations automatically on startup
- Seed data runs automatically when `ASPNETCORE_ENVIRONMENT=Development`
- Docker Compose injects the SQL Server connection string and other settings through environment variables, so you do not need to edit the tracked appsettings files just to run containers

To stop the stack:

```bash
docker compose down
```

To stop the stack and remove the persisted SQL Server volume:

```bash
docker compose down -v
```

## Deploy To Render

This repository can be deployed to Render without running Docker on your own machine. Render will build the app directly from the repo using `Presentation/Dockerfile`.

Files included for Render:

- `render.yaml` - Render Blueprint definition for the web service
- `Presentation/start.sh` - startup script that binds ASP.NET Core to Render's runtime `PORT`

Important deployment note:

- This app uses SQL Server
- Render does not provide a managed SQL Server database
- You should point `ConnectionStrings__DefaultConnection` to an external SQL Server instance such as Azure SQL or another hosted SQL Server

### Render Setup

1. Push this repository to GitHub
2. In Render, create a new Blueprint or Web Service from the repo
3. Let Render detect `render.yaml`
4. Set the secret environment variables in Render:

- `ConnectionStrings__DefaultConnection`
- `CloudinarySettings__CloudName`
- `CloudinarySettings__ApiKey`
- `CloudinarySettings__ApiSecret`
- `MailSettings__FromEmail`
- `MailSettings__Password`
- `MailSettings__SmtpHost` if you are not using Gmail
- `MailSettings__SmtpPort` if you are not using port `587`

5. Deploy the service

Render will automatically provide the runtime `PORT`, and the container startup script will bind the ASP.NET Core app to that port.

## Run Without Docker

Build the solution:

```bash
dotnet restore
dotnet build StaffHubSystem.sln
```

Apply migrations manually if needed:

```bash
dotnet ef database update --project Data/Data.csproj --startup-project Presentation/Presentation.csproj
```

Run the app:

```bash
dotnet run --project Presentation/Presentation.csproj
```

Then open the app at `https://localhost:5001` or `http://localhost:5000`, depending on your local launch settings.

## Notes

- ASP.NET Core Identity is configured with strong password requirements
- Secrets should be supplied through environment variables or local secrets, not committed appsettings files
