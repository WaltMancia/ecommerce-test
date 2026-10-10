using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;

namespace SecureShop.Infrastructure.Services;

public class PaymentGatewayClient
{
    private readonly HttpClient _httpClient;

    public PaymentGatewayClient(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(config["ExternalServices:PaymentSimulator"]!);
    }

    public async Task<(bool Success, string TransactionId)> ProcessPaymentAsync(int orderId, decimal amount)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/payments", new { orderId, amount });
        if (!response.IsSuccessStatusCode) return (false, string.Empty);

        var result = await response.Content.ReadFromJsonAsync<PaymentResult>();
        return (result?.Status == "APPROVED", result?.TransactionId ?? "");
    }
    private record PaymentResult(string Status, string TransactionId);
}

public class NotificationClient
{
    private readonly HttpClient _httpClient;

    public NotificationClient(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(config["ExternalServices:NotificationSimulator"]!);
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        await _httpClient.PostAsJsonAsync("/api/notifications", new { to, subject, body });
    }
}