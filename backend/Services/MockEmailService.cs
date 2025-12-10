namespace todo_app_teldat.Services;

public class MockEmailService : IEmailService
{
    private readonly ILogger<MockEmailService> _logger;

    public MockEmailService(ILogger<MockEmailService> logger)
    {
        _logger = logger;
    }

    public Task SendReminderEmailAsync(string email, string todoTitle, DateTime dueDate)
    {
        _logger.LogInformation(
            "[MOCK EMAIL] Sending reminder to {Email}: Task '{Title}' is due on {DueDate}",
            email,
            todoTitle,
            dueDate.ToString("yyyy-MM-dd"));
        
        return Task.CompletedTask;
    }
}
