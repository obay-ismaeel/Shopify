FROM mcr.microsoft.com/dotnet/sdk:8.0 AS restore
WORKDIR /src

COPY ["Shopify.sln", "./"]

COPY ["src/Shared/Shopify.Contracts/Shopify.Contracts.csproj", "src/Shared/Shopify.Contracts/"]

COPY ["src/Shopify.NotificationService/Shopify.NotificationService.API/Shopify.NotificationService.API.csproj", "src/Shopify.NotificationService/Shopify.NotificationService.API/"]
COPY ["src/Shopify.NotificationService/Shopify.NotificationService.Application/Shopify.NotificationService.Application.csproj", "src/Shopify.NotificationService/Shopify.NotificationService.Application/"]
COPY ["src/Shopify.NotificationService/Shopify.NotificationService.Domain/Shopify.NotificationService.Domain.csproj", "src/Shopify.NotificationService/Shopify.NotificationService.Domain/"]
COPY ["src/Shopify.NotificationService/Shopify.NotificationService.Infrastructure/Shopify.NotificationService.Infrastructure.csproj", "src/Shopify.NotificationService/Shopify.NotificationService.Infrastructure/"]

RUN dotnet restore "src/Shopify.NotificationService/Shopify.NotificationService.API/Shopify.NotificationService.API.csproj"

FROM restore AS dev
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /src

FROM restore AS publish
COPY . .
RUN dotnet publish "src/Shopify.NotificationService/Shopify.NotificationService.API/Shopify.NotificationService.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/* \
    && addgroup --system appgroup \
    && adduser --system --ingroup appgroup appuser

COPY --from=publish /app/publish .
RUN chown -R appuser:appgroup /app

USER appuser

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV DOTNET_RUNNING_IN_CONTAINER=true

HEALTHCHECK --interval=15s --timeout=5s --start-period=30s --retries=3 \
    CMD curl -fsS http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "Shopify.NotificationService.API.dll"]
