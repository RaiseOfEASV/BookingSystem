using BookingService.Application.Options;
using BookingService.Infrastructure;
using BookingService.Infrastructure.Persistence;
using MessageClient.Configuration;
using MessageClient.Extension;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Booking Service API", Version = "v1" });
});

builder.Services.AddOptions<BookingServiceOptions>()
    .Bind(builder.Configuration.GetSection(BookingServiceOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddOptions<OutboxWorkerOptions>()
    .Bind(builder.Configuration.GetSection(OutboxWorkerOptions.SectionName))
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
    var db = scope.ServiceProvider.GetRequiredService<BookingDbContext>();
    db.Database.Migrate();
}

app.UseHttpsRedirection();
app.MapControllers();
app.Run();
