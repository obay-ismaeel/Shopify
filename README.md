# Shopify E-Commerce Microservices

A .NET 8 e-commerce system built on event-driven microservices. Three independent services communicate exclusively through RabbitMQ — no service ever calls another directly.

---

## Architecture

```

                          ┌─────────────────┐
                          │  Order Service  │
                          │  ASP.NET Core   │
                          │  orders_db      │
                          └────────▲────────┘
                                   │
                                   │ publishes / consumes
                                   ▼
                           ╔═════════════════╗
                           ║     RabbitMQ    ║
                           ║   (Event Bus)   ║
                           ╚═════════════════╝
                            ▲              │      
                            │              │      
                    ┌───────┼              ┼───────┐
publishes/consumes  │                              │ consumes
                    │                              │
                    ▼                              ▼
        ┌──────────────────┐            ┌──────────────────────┐
        │ Inventory Service│            │ Notification Service │
        │ ASP.NET Core API │            │ .NET Worker          │
        │                  │            │ notifications_db     │
        │ inventory_db     │            └──────────────────────┘
        └──────────────────┘

```

Each service owns its own database. Communication happens only through events. Services can be deployed, scaled, and failed independently.

---

## Project Structure

Each service follows **Clean Architecture** with four layers enforced by separate `.csproj` files

```
src/
├── Shared/
│   └── Shared.Contracts/            # Integration event definitions (shared between services)
│
├── OrderService/
│   ├── Shopify.OrderService.Domain/         # Entities, domain events — zero external dependencies
│   ├── Shopify.OrderService.Application/    # Commands, queries, MediatR handlers, interfaces
│   ├── Shopify.OrderService.Infrastructure/ # EF Core, repositories, outbox, consumers
│   └── Shopify.OrderService.API/            # Controllers, middleware, Program.cs
│
├── InventoryService/
│   ├── Shopify.InventoryService.Domain/
│   ├── Shopify.InventoryService.Application/
│   ├── Shopify.InventoryService.Infrastructure/
│   └── Shopify.InventoryService.API/   
│
└── NotificationService/
    ├── Shopify.NotificationService.Domain/
    ├── Shopify.NotificationService.Application/
    ├── Shopify.NotificationService.Infrastructure/
    └── Shopify.NotificationService.API/
```

---

## Services

### Order Service

Accepts order requests from clients and drives the workflow. Publishes `OrderCreatedIntegrationEvent` via the Outbox Pattern, then listens for inventory results and updates the order status accordingly.

| Method | Endpoint | Description |
|--------|----------|-------------|
| `POST` | `/api/orders` | Create a new order |
| `GET` | `/api/orders/{id}` | Get order by ID |

**Order status lifecycle:**
```
Pending → Confirmed   (inventory reserved)
        → Cancelled   (out of stock)
```

---

### Inventory Service

The stock gatekeeper. Consumes `OrderCreatedIntegrationEvent`, runs the reservation business rule inside the `Product` aggregate, and publishes the outcome. Also exposes a read API for browsing product stock levels.

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/api/products` | List all products (paginated) |
| `GET` | `/api/products/{id}` | Get product by ID |

**Seeded test products:**

| Product ID | Name | Stock |
|-----------|------|-------|
| `...0001` | Widget A | 100 |
| `...0002` | Widget B | 50 |
| `...0003` | Widget C | 3 (low stock) |
| `...0004` | Widget D | 0 (out of stock) |

---

### Notification Service

Consumes inventory outcome events and dispatches notifications. Implemented through an `INotificationSender` interface — swapping to email, SMS, or push notifications requires changing a single DI registration with no other code changes.

---

## Message Flow

```
1.  POST /api/orders
          │
          ▼
2.  CreateOrderCommandHandler
    ├── Checks idempotency key (prevents duplicate orders)
    ├── Order.Create() → raises OrderCreatedDomainEvent
    └── SaveChangesAsync() ── one atomic transaction ──────────┐
                                                               ├── orders table
                                                               ├── outbox_messages table
                                                               └── idempotency_keys table

3.  OutboxPublisherService (polls every 5s)
    └── maps OrderCreatedDomainEvent → OrderCreatedIntegrationEvent → RabbitMQ

4.  RabbitMQ fans out to OrderCreated queue

5.  OrderCreatedConsumer → ReserveStockCommandHandler
    ├── Checks ProcessedOrders table (prevents duplicate stock deductions)
    ├── product.Reserve(quantity, orderId)
    │     ├── Stock OK  → StockReservedDomainEvent
    │     └── No stock  → StockReservationFailedDomainEvent
    └── SaveChangesAsync() ── one atomic transaction ──────────┐
                                                               ├── products table (stock updated)
                                                               ├── processed_orders table
                                                               └── outbox_messages table

6.  OutboxPublisherService (polls every 5s)
    ├── StockReservedDomainEvent          → InventoryUpdatedIntegrationEvent
    └── StockReservationFailedDomainEvent → OutOfStockIntegrationEvent

7.  RabbitMQ fans one of the two events out to two queues (a queue for each service):

    Order Service                        Notification Service
          │                                      │
          ▼                                      ▼
    Updates order status              Sends notification to user
    Pending → Confirmed               ✅ "Your order is confirmed"
    Pending → Cancelled               ⚠️  "Your order is rejected"
```

---

## Design Choices

### Outbox Pattern
Events are written to an `outbox_messages` table in the same database transaction as the entity change, then published to RabbitMQ by a background `OutboxPublisherService`. This eliminates the dual-write problem — if the service crashes after saving the entity but before reaching the broker, the event is not lost. Delivery is guaranteed.

### Domain Events vs Integration Events
Services raise internal **domain events** (e.g. `StockReservedDomainEvent`) that stay within the service boundary. The Outbox Publisher maps these to **integration events** from `Shared.Contracts` before publishing to the broker. Internal domain models can evolve freely without breaking the contracts other services depend on.

### CQRS with MediatR
Commands and queries are separated into dedicated handler classes. Cross-cutting concerns — logging and validation — are MediatR pipeline behaviors that run automatically on every request, keeping handlers focused purely on business logic with no boilerplate.

### Idempotency
Duplicate handling is enforced at two levels:

- **Order Service (HTTP):** Client supplies an `Idempotency-Key` UUID header. The key is stored atomically with the order. Duplicate requests return the original response without creating a new order.
- **Inventory Service (events):** Every processed `OrderId` is recorded in a `processed_orders` table. Duplicate event deliveries are detected and skipped before any stock is touched.

### Optimistic Concurrency
`Product` uses PostgreSQL's `xmin` system column as a concurrency token. EF Core includes it in every `UPDATE` statement. If two consumers attempt to reserve stock simultaneously, the second writer receives a `DbUpdateConcurrencyException`, retries, and reads the correct updated stock level.

---

## Error Handling & Fault Tolerance

### Message Acknowledgment
Messages are acknowledged to RabbitMQ only after a consumer completes successfully. If the consumer throws at any point, the message is returned to the queue for retry — nothing is silently dropped.

### Consumer Retry Policy
MassTransit applies automatic retry to every consumer across all three services:
```
Retry intervals: 1s → 5s → 15s
```
After all retries are exhausted, the message is moved to a dead-letter error queue (`*_error`) for manual inspection.

### Outbox Retry
If `OutboxPublisherService` fails to reach the broker, it increments `retry_count` on the outbox row and retries on the next poll cycle. Failed rows are preserved with the error recorded.

### Notification Retry Service
The Notification Service runs a dedicated `RetryFailedNotificationsService` background worker that polls every minute for notifications with `Status = Failed` and re-attempts delivery. This handles the case where the event was already acknowledged from the broker but `INotificationSender` threw afterwards — a failure that consumer-level retry cannot address.

### Global Exception Handling
Both API services use `ExceptionHandlingMiddleware` mapping typed exceptions:

| Exception | HTTP Status |
|-----------|------------|
| `ValidationException` | 400 Bad Request |
| Domain exceptions | 400 Bad Request |
| `NotFoundException` | 404 Not Found |
| Unique constraint violation | 409 Conflict |
| Unhandled | 500 Internal Server Error |


---

## Tech Stack

| | Technology |
|-|------------|
| **Runtime** | .NET 8 |
| **API** | ASP.NET Core |
| **Messaging** | MassTransit + RabbitMQ |
| **CQRS** | MediatR |
| **Validation** | FluentValidation |
| **ORM** | Entity Framework Core 8 |
| **Database** | PostgreSQL 16 |
| **Containers** | Docker + Docker Compose |

---

## Setup & Running

Services wait for PostgreSQL and RabbitMQ healthchecks before starting. Migrations and seed data are applied automatically on startup.

### Dockerfiles

There is one Dockerfile per service:

- `src/Shopify.OrderService/Dockerfile`
- `src/Shopify.InventoryService/Dockerfile`
- `src/Shopify.NotificationService/Dockerfile`

`docker-compose.yml` builds each service using the repository root as the build context so the shared contracts project can be restored.

### Run with Docker Compose

From the repository root:

```bash
docker compose build
docker compose up -d
```

To stop:

```bash
docker compose down
```

### Development (hot reload)

This repository includes `docker-compose.override.yml` which runs each service using `dotnet watch` and mounts the source code into the containers.

```bash
docker compose up --build
```

### Default credentials

- PostgreSQL:
  - user: `postgres`
  - password: `postgres`
- RabbitMQ Management UI:
  - user: `guest`
  - password: `guest`

### URLs

| URL | Description |
|-----|-------------|
| `http://localhost:5001/swagger` | Order Service Swagger UI |
| `http://localhost:5001/health` | Order Service healthcheck |
| `http://localhost:5002/swagger` | Inventory Service Swagger UI |
| `http://localhost:5002/health` | Inventory Service healthcheck |
| `http://localhost:5003/swagger` | Notification Service Swagger UI |
| `http://localhost:5003/health` | Notification Service healthcheck |
| `http://localhost:15672` | RabbitMQ Management UI (`guest` / `guest`) |
