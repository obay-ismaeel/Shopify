using Shopify.OrderService.API.Middlewares;
using Shopify.OrderService.Application;
using Shopify.OrderService.Infrastructure;
using Shopify.OrderService.Infrastructure.Outbox;

var builder = WebApplication.CreateBuilder(args);

// services

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opts =>
{
    opts.SwaggerDoc("v1", new()
    {
        Title = "Order Service API",
        Version = "v1",
        Description = "Manages order creation and publishes OrderCreated events via the Outbox Pattern."
    });
    //opts.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "OrderService.API.xml"), true);
});

builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("Default")!, name: "postgres")
    .AddRabbitMQ(rabbitConnectionString: $"amqp://{builder.Configuration["RabbitMq:Host"]}", name: "rabbitmq");

builder.Services.AddHostedService<OutboxPublisherService>();

var app = builder.Build();

// pipeline 

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/health");

await app.Services.ApplyMigrationsAsync();

app.Run();
