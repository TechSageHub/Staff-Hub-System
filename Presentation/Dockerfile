FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

ENV ASPNETCORE_HTTP_PORTS=8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["Presentation/Presentation.csproj", "Presentation/"]
COPY ["Application/Application.csproj", "Application/"]
COPY ["Data/Data.csproj", "Data/"]
RUN dotnet restore "Presentation/Presentation.csproj"

COPY . .
WORKDIR "/src/Presentation"
RUN dotnet build "Presentation.csproj" -c $BUILD_CONFIGURATION -o /app/build /p:KillLockedPresentationProcessBeforeBuild=false

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Presentation.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false /p:KillLockedPresentationProcessBeforeBuild=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY ["Presentation/start.sh", "./start.sh"]
RUN chmod +x ./start.sh
ENTRYPOINT ["./start.sh"]
