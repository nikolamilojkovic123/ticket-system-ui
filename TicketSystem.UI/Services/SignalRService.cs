using Blazored.LocalStorage;
using Microsoft.AspNetCore.SignalR.Client;
using TicketSystem.UI.Models;

namespace TicketSystem.UI.Services;

public sealed class SignalRService(
    ILocalStorageService localStorage,
    NotificationService notificationService,
    IConfiguration configuration) : IAsyncDisposable
{
    private HubConnection? _connection;

    public bool IsConnected => _connection?.State == HubConnectionState.Connected;

    public async Task StartAsync()
    {
        if (_connection is not null) return;

        string? token = await localStorage.GetItemAsync<string>("authToken");
        if (string.IsNullOrEmpty(token)) return;

        string baseUrl = (configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5000").TrimEnd('/');

        _connection = new HubConnectionBuilder()
            .WithUrl($"{baseUrl}/hubs/tickets", options =>
            {
                options.AccessTokenProvider = () => Task.FromResult<string?>(token);
            })
            .WithAutomaticReconnect()
            .Build();

        _connection.On<string, string>("TicketAssigned", (ticketId, title) =>
        {
            Guid.TryParse(ticketId, out var id);
            notificationService.Push(
                "Ticket Assigned",
                $"Ticket \"{title}\" has been assigned to you",
                NotificationType.Info,
                id == Guid.Empty ? null : id);
        });

        _connection.On<string, string>("TicketUpdated", (ticketId, title) =>
        {
            Guid.TryParse(ticketId, out var id);
            notificationService.Push(
                "Ticket Updated",
                $"Ticket \"{title}\" has been updated",
                NotificationType.Warning,
                id == Guid.Empty ? null : id);
        });

        _connection.On<long, object>("CommentAdded", (caseId, comment) =>
        {
            notificationService.Push(
                "New Comment",
                $"A new comment was added to ticket #{caseId}",
                NotificationType.Info);
        });

        _connection.Closed += async (error) =>
        {
            await Task.Delay(TimeSpan.FromSeconds(5));
            await TryReconnectAsync();
        };

        await TryConnectAsync();
    }

    public async Task StopAsync()
    {
        if (_connection is not null)
        {
            await _connection.StopAsync();
            await _connection.DisposeAsync();
            _connection = null;
        }
    }

    private async Task TryConnectAsync()
    {
        try
        {
            await _connection!.StartAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SignalR] Connection failed: {ex.Message}");
        }
    }

    private async Task TryReconnectAsync()
    {
        if (_connection is null) return;
        try
        {
            await _connection.StartAsync();
        }
        catch
        {
            // silent — WithAutomaticReconnect handles most cases
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection is not null)
            await _connection.DisposeAsync();
    }
}
