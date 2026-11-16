
using AtlasPay.Notification.Application.Abstractions.Persistence;
using AtlasPay.Notification.Infrastructure.Persistence;
using AtlasPay.Notification.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AtlasPay.Notification.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration
            .GetConnectionString("NotificationDb");

        services.AddDbContext<NotificationDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IUnitOfWork>(sp =>
            sp.GetRequiredService<NotificationDbContext>());

        services.AddScoped<INotificationPreferenceRepository, NotificationPreferenceRepository>();
        services.AddScoped<INotificationPreferenceReadRepository, NotificationPreferenceReadRepository>();
        services.AddScoped<INotificationChannelRepository, NotificationChannelRepository>();
        services.AddScoped<INotificationChannelReadRepository, NotificationChannelReadRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<IProcessedEventRepository, ProcessedEventRepository>();
        services.AddScoped<IAccountNotificationPreferenceRepository, AccountNotificationPreferenceRepository>();
        services.AddScoped<IAccountNotificationPreferenceReadRepository, AccountNotificationPreferenceReadRepository>();
        
        return services;
    }
}
