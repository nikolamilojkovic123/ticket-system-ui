using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using TicketSystem.UI.Interfaces;
using TicketSystem.UI.Services;

namespace TicketSystem.UI.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<ITicketService, TicketService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<CultureService>();
        services.AddScoped<ThemeService>();
        services.AddSingleton<ToastService>();
        services.AddSingleton<NotificationService>();
        services.AddScoped<SignalRService>();
        return services;
    }

    public static IServiceCollection AddApiClients(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddTransient<AuthorizationHandler>();

        services.AddHttpClient("BackendAPI", client =>
        {
            client.BaseAddress = new Uri(configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5000");
        })
        .AddHttpMessageHandler<AuthorizationHandler>();

        return services;
    }

    public static IServiceCollection AddAuth(this IServiceCollection services)
    {
        services.AddAuthorizationCore();
        services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();

        return services;
    }

    public static IServiceCollection AddLocalizationServices(this IServiceCollection services)
    {
        services.AddBlazoredLocalStorage();
        services.AddLocalization();

        return services;
    }
}