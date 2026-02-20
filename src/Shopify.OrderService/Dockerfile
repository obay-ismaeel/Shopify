FROM mcr.microsoft.com/dotnet/sdk:8.0 AS restore
WORKDIR /src

COPY ["Shopify.sln", "./"]

COPY ["src/Shared/Shopify.Contracts/Shopify.Contracts.csproj", "src/Shared/Shopify.Contracts/"]

COPY ["src/Shopify.OrderService/Shopify.OrderService.API/Shopify.OrderService.API.csproj", "src/Shopify.OrderService/Shopify.OrderService.API/"]
COPY ["src/Shopify.OrderService/Shopify.OrderService.Application/Shopify.OrderService.Application.csproj", "src/Shopify.OrderService/Shopify.OrderService.Application/"]
COPY ["src/Shopify.OrderService/Shopify.OrderService.Domain/Shopify.OrderService.Domain.csproj", "src/Shopify.OrderService/Shopify.OrderService.Domain/"]
COPY ["src/Shopify.OrderService/Shopify.OrderService.Infrastructure/Shopify.OrderService.Infrastructure.csproj", "src/Shopify.OrderService/Shopify.OrderService.Infrastructure/"]

RUN dotnet restore "src/Shopify.OrderService/Shopify.OrderService.API/Shopify.OrderService.API.csproj"

FROM restore AS publish
COPY . .
RUN dotnet publish "src/Shopify.OrderService/Shopify.OrderService.API/Shopify.OrderService.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

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

ENTRYPOINT ["dotnet", "Shopify.OrderService.API.dll"]
