using InventoryService.Infrastructure;
using InventoryService.Infrastructure.Options;
using InventoryService.Infrastructure.Persistence;
using MessageClient.Configuration;
using MessageClient.Extension;
using Microsoft.EntityFrameworkCore;
using VaultClient.Extension;

var builder = WebApplication.CreateBuilder(args);

await builder.ConfigureFromVaultAsync("inventory-service");

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Inventory Service API", Version = "v1" });
});
builder.Services
    .AddOptions<MessageProcessingOptions>()
    .BindConfiguration(MessageProcessingOptions.SectionName)
    .ValidateDataAnnotations()
    .ValidateOnStart();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddRabbitMqMessageClient(
    new RabbitMqClientOptions { ConnectionString = builder.Configuration["RabbitMQ:ConnectionString"]! },
    new MessageHandlerOptions { SubscriptionPrefix = builder.Configuration["RabbitMQ:SubscriptionPrefix"]! }
);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
    db.Database.Migrate();
}

app.UseHttpsRedirection();
app.MapControllers();
app.Run();
