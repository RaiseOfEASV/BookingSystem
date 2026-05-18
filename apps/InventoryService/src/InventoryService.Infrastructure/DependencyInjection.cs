using InventoryService.Application.Interfaces;
using InventoryService.Application.Services;
using InventoryService.Infrastructure.Persistence;
using InventoryService.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace InventoryService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<InventoryDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IResourceRepository, ResourceRepository>();
        services.AddScoped<IResourceService, ResourceService>();

        return services;
    }
}
