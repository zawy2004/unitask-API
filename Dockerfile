FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY src/Unitask.Api/Unitask.Api.csproj                     src/Unitask.Api/
COPY src/Unitask.Application/Unitask.Application.csproj     src/Unitask.Application/
COPY src/Unitask.Domain/Unitask.Domain.csproj               src/Unitask.Domain/
COPY src/Unitask.Infrastructure/Unitask.Infrastructure.csproj src/Unitask.Infrastructure/
RUN dotnet restore src/Unitask.Api/Unitask.Api.csproj

COPY src/ src/
RUN dotnet publish src/Unitask.Api/Unitask.Api.csproj \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build --chown=app:app /app/publish ./
USER app

ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_RUNNING_IN_CONTAINER=true \
    DOTNET_EnableDiagnostics=0

EXPOSE 8080

HEALTHCHECK --interval=30s --timeout=5s --start-period=20s --retries=3 \
    CMD wget -q -O- http://127.0.0.1:8080/health || exit 1

ENTRYPOINT ["dotnet", "Unitask.Api.dll"]
