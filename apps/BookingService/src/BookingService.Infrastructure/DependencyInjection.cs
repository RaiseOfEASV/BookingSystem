using BookingService.Application.Interfaces;
using BookingService.Application.Sagas;
using BookingService.Application.Services;
using BookingService.Infrastructure.BackgroundWorkers;
using BookingService.Infrastructure.Caching;
using BookingService.Infrastructure.Persistence;
using BookingService.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace BookingService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<BookingDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddScoped<IBookingService, Application.Services.BookingService>();

        // Redis
        var redisConnection = configuration.GetConnectionString("Redis") ?? "localhost:6379";
        services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisConnection));
        services.AddSingleton<ISeatAvailabilityCache, SeatAvailabilityCache>();
        services.AddScoped<ISeatReservationService, SeatReservationService>();
        services.AddScoped<BookingSagaOrchestrator>();

        services.AddHostedService<OutboxWorker>();

        return services;
    }
}
