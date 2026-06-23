using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using TicketSystem.UI;
using TicketSystem.UI.Extensions;
using System.Globalization;

WebAssemblyHostBuilder builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Core HttpClient (Blazor default)
builder.Services.AddScoped(sp =>
    new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Modular DI
builder.Services
    .AddApiClients(builder.Configuration)
    .AddApplicationServices()
    .AddAuth()
    .AddLocalizationServices();

WebAssemblyHost host = builder.Build();

// Culture init
using (IServiceScope scope = host.Services.CreateScope())
{
    ISyncLocalStorageService localStorage = scope.ServiceProvider.GetRequiredService<ISyncLocalStorageService>();
    var cultureName = localStorage.GetItem<string>("Language") ?? "sr";

    CultureInfo culture = new(cultureName);

    CultureInfo.DefaultThreadCurrentCulture = culture;
    CultureInfo.DefaultThreadCurrentUICulture = culture;
}

await host.RunAsync();