using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace RfidWarehouseApi.Services;

public interface IEmailService
{
    Task SendOverdueItemEmailAsync(
        string recipientEmail,
        string recipientName,
        string itemName,
        int daysOverdue,
        DateTime borrowedDate,
        CancellationToken cancellationToken = default);
    
    Task<bool> SendEmailAsync(
        string recipientEmail,
        string subject,
        string textBody,
        CancellationToken cancellationToken = default);
}

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;
    private readonly HttpClient _httpClient;
    private readonly string? _apiToken;
    private readonly long _inboxId;
    private readonly string _fromEmail;
    private readonly string _fromName;
    private readonly string _baseUrl;

    public EmailService(ILogger<EmailService> logger, HttpClient httpClient, IConfiguration configuration)
    {
        _logger = logger;
        _httpClient = httpClient;

        _apiToken = configuration["Mailtrap:ApiToken"]; // keep optional so API can boot without secrets

        var inboxIdRaw = configuration["Mailtrap:InboxId"];
        if (!long.TryParse(inboxIdRaw, out _inboxId) || _inboxId <= 0)
        {
            _inboxId = 0;
        }

        _baseUrl = configuration["Mailtrap:BaseUrl"]?.TrimEnd('/')
            ?? "https://sandbox.api.mailtrap.io";

        _fromEmail = configuration["Mailtrap:FromEmail"] ?? "hello@tooltrackpro.com";
        _fromName = configuration["Mailtrap:FromName"] ?? "Tool Track Pro";

        _httpClient.DefaultRequestHeaders.Accept.Clear();
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public async Task SendOverdueItemEmailAsync(
        string recipientEmail,
        string recipientName,
        string itemName,
        int daysOverdue,
        DateTime borrowedDate,
        CancellationToken cancellationToken = default)
    {
        var subject = $"⚠️ Overdue Item Reminder: {itemName}";
        var textBody = $@"Dear {recipientName},

This is a friendly reminder that you have an item that is overdue for return.

📦 Item: {itemName}
📅 Borrowed on: {borrowedDate:MMMM dd, yyyy}
⏰ Days overdue: {daysOverdue} days

Please return this item to the warehouse as soon as possible to avoid any inconvenience for other team members who may need it.

If you have already returned this item or have any questions, please contact the warehouse administrator.

Thank you for your cooperation!

Best regards,
ToolTrackPro Warehouse Management System

---
This is an automated message from the ToolTrackPro Warehouse Management System.";

        await SendEmailAsync(recipientEmail, subject, textBody, cancellationToken);
    }

    public async Task<bool> SendEmailAsync(
        string recipientEmail,
        string subject,
        string textBody,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(_apiToken) || _inboxId <= 0)
            {
                _logger.LogError(
                    "Mailtrap sandbox email is not configured. Set Mailtrap:ApiToken and Mailtrap:InboxId (user-secrets recommended). InboxId={InboxId}.",
                    _inboxId);
                return false;
            }

            var payload = new
            {
                from = new { email = _fromEmail, name = _fromName },
                to = new[] { new { email = recipientEmail } },
                subject = subject,
                text = textBody,
                category = "Overdue Reminder"
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Sandbox send endpoint (per official OpenAPI):
            // POST https://sandbox.api.mailtrap.io/api/send/{inbox_id}
            var url = $"{_baseUrl}/api/send/{_inboxId}";
            using var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = content
            };

            // Stoplight Sandbox API docs specify Api-Token header.
            request.Headers.TryAddWithoutValidation("Api-Token", _apiToken);

            var response = await _httpClient.SendAsync(request, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogInformation("Email sent successfully to {Email} via Mailtrap sandbox. Response: {Response}", 
                    recipientEmail, responseBody);
                return true;
            }
            else
            {
                var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Failed to send email to {Email}. Status: {Status}, Error: {Error}", 
                    recipientEmail, response.StatusCode, errorBody);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email} via Mailtrap", recipientEmail);
            return false;
        }
    }
}
