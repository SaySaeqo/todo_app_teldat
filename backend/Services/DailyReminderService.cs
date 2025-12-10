using Microsoft.EntityFrameworkCore;
using todo_app_teldat.Data;

namespace todo_app_teldat.Services;

public class DailyReminderService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DailyReminderService> _logger;

    public DailyReminderService(
        IServiceProvider serviceProvider,
        ILogger<DailyReminderService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Daily Reminder Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.UtcNow;
            var nextMidnight = now.Date.AddDays(1);
            var delay = nextMidnight - now;

            _logger.LogInformation("Next reminder check scheduled at {NextMidnight} (in {Delay})", 
                nextMidnight, delay);

            try
            {
                await Task.Delay(delay, stoppingToken);
                await SendRemindersAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Daily Reminder Service is stopping");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Daily Reminder Service");
            }
        }
    }

    private async Task SendRemindersAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Running daily reminder check at {Time}", DateTime.UtcNow);

        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

        var today = DateTime.UtcNow.Date;
        var threeDaysFromNow = today.AddDays(3).AddDays(1); // End of 3rd day
        
        var upcomingTasks = await context.TodoItems
            .Include(t => t.SubscriptionEmails)
            .Where(t => !t.IsComplete 
                && t.DueDate.HasValue 
                && t.DueDate.Value >= today
                && t.DueDate.Value < threeDaysFromNow)
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Found {Count} tasks due within 3 days", upcomingTasks.Count);

        foreach (var task in upcomingTasks)
        {
            if (task.SubscriptionEmails == null || !task.SubscriptionEmails.Any())
            {
                continue;
            }

            foreach (var subscription in task.SubscriptionEmails)
            {
                try
                {
                    await emailService.SendReminderEmailAsync(
                        subscription.Email,
                        task.Title,
                        task.DueDate!.Value);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send reminder email to {Email} for task {TaskId}", 
                        subscription.Email, task.Id);
                }
            }
        }

        _logger.LogInformation("Daily reminder check completed");
    }
}
