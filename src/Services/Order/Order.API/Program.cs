using BuildingBlocks.Integration.EventBus;
using Microsoft.EntityFrameworkCore;
using Order.Application.Handlers;
using Order.Application.IntegrationEventHandlers;
using Order.Domain.Repositories;
using Order.Infrastructure.Messaging;
using Order.Infrastructure.Persistence;
using Order.Infrastructure.Repositories;
using Payment.IntegrationEvents;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container

// DbContext with In-Memory database for demo
builder.Services.AddDbContext<OrderDbContext>(options =>
    options.UseInMemoryDatabase("OrderDb"));

// MediatR for CQRS
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(CreateOrderCommandHandler).Assembly);
});

// Repository
builder.Services.AddScoped<IOrderRepository, OrderRepository>();

// Event Bus
builder.Services.AddSingleton<IEventBus, InMemoryEventBus>();

// Integration Event Handlers
builder.Services.AddScoped<IIntegrationEventHandler<PaymentCompletedIntegrationEvent>, PaymentCompletedIntegrationEventHandler>();
builder.Services.AddScoped<IIntegrationEventHandler<PaymentFailedIntegrationEvent>, PaymentFailedIntegrationEventHandler>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Order Service API", Version = "v1" });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Subscribe to integration events
var eventBus = app.Services.GetRequiredService<IEventBus>();
eventBus.Subscribe<PaymentCompletedIntegrationEvent, PaymentCompletedIntegrationEventHandler>();
eventBus.Subscribe<PaymentFailedIntegrationEvent, PaymentFailedIntegrationEventHandler>();

app.Run();
