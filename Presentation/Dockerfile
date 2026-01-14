# Base runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

# Render provides PORT env var, default to 5000 if not set
ENV ASPNETCORE_URLS=http://+:${PORT:-5000}
EXPOSE 5000

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Presentation/Presentation.csproj", "Presentation/"]
COPY ["Application/Application.csproj", "Application/"]
COPY ["Data/Data.csproj", "Data/"]
RUN dotnet restore "./Presentation/Presentation.csproj"
COPY . .
WORKDIR "/src/Presentation"
RUN dotnet build "./Presentation.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Presentation.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final runtime stage
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Presentation.dll"]
