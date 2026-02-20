FROM mcr.microsoft.com/dotnet/sdk:8.0 AS restore
WORKDIR /src

COPY ["Shopify.sln", "./"]

COPY ["src/Shared/Shopify.Contracts/Shopify.Contracts.csproj", "src/Shared/Shopify.Contracts/"]

COPY ["src/Shopify.InventoryService/Shopify.InventoryService.API/Shopify.InventoryService.API.csproj", "src/Shopify.InventoryService/Shopify.InventoryService.API/"]
COPY ["src/Shopify.InventoryService/Shopify.InventoryService.Application/Shopify.InventoryService.Application.csproj", "src/Shopify.InventoryService/Shopify.InventoryService.Application/"]
COPY ["src/Shopify.InventoryService/Shopify.InventoryService.Domain/Shopify.InventoryService.Domain.csproj", "src/Shopify.InventoryService/Shopify.InventoryService.Domain/"]
COPY ["src/Shopify.InventoryService/Shopify.InventoryService.Infrastructure/Shopify.InventoryService.Infrastructure.csproj", "src/Shopify.InventoryService/Shopify.InventoryService.Infrastructure/"]

RUN dotnet restore "src/Shopify.InventoryService/Shopify.InventoryService.API/Shopify.InventoryService.API.csproj"

FROM restore AS publish
COPY . .
RUN dotnet publish "src/Shopify.InventoryService/Shopify.InventoryService.API/Shopify.InventoryService.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

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

ENTRYPOINT ["dotnet", "Shopify.InventoryService.API.dll"]
