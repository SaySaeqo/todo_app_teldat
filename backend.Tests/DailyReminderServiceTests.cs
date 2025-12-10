using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using todo_app_teldat.Data;
using todo_app_teldat.Models;
using todo_app_teldat.Services;
using Xunit;

namespace backend.Tests;

public class DailyReminderServiceTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly string _databaseName;

    public DailyReminderServiceTests()
    {
        _databaseName = $"TestDb_{Guid.NewGuid()}";
        var services = new ServiceCollection();
        
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseInMemoryDatabase(_databaseName));
        
        services.AddLogging(builder => builder.AddConsole());
        services.AddSingleton<IEmailService, TestEmailService>();
        
        _serviceProvider = services.BuildServiceProvider();
    }

    public void Dispose()
    {
        _serviceProvider?.Dispose();
    }

    [Fact]
    public async Task SendReminders_SendsEmailForTasksDueWithin3Days()
    {
        // Arrange
        var emailService = (TestEmailService)_serviceProvider.GetRequiredService<IEmailService>();
        var today = DateTime.UtcNow.Date;
        
        using (var scope = _serviceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            
            var taskDueTomorrow = new TodoItem
            {
                Title = "Task Due Tomorrow",
                IsComplete = false,
                DueDate = today.AddDays(1),
                CreatedAt = DateTime.UtcNow,
                SubscriptionEmails = new List<SubscriptionEmail>
                {
                    new() { Email = "user1@example.com" },
                    new() { Email = "user2@example.com" }
                }
            };

            var taskDueIn3Days = new TodoItem
            {
                Title = "Task Due In 3 Days",
                IsComplete = false,
                DueDate = today.AddDays(3),
                CreatedAt = DateTime.UtcNow,
                SubscriptionEmails = new List<SubscriptionEmail>
                {
                    new() { Email = "user3@example.com" }
                }
            };

            context.TodoItems.AddRange(taskDueTomorrow, taskDueIn3Days);
            await context.SaveChangesAsync();
        }

        // Act
        await InvokeSendRemindersAsync();

        // Assert
        Assert.Equal(3, emailService.SentEmails.Count);
        Assert.Contains(emailService.SentEmails, e => e.Email == "user1@example.com" && e.Title == "Task Due Tomorrow");
        Assert.Contains(emailService.SentEmails, e => e.Email == "user2@example.com" && e.Title == "Task Due Tomorrow");
        Assert.Contains(emailService.SentEmails, e => e.Email == "user3@example.com" && e.Title == "Task Due In 3 Days");
    }

    [Fact]
    public async Task SendReminders_DoesNotSendForCompletedTasks()
    {
        // Arrange
        var emailService = (TestEmailService)_serviceProvider.GetRequiredService<IEmailService>();
        var today = DateTime.UtcNow.Date;
        
        using (var scope = _serviceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            
            var completedTask = new TodoItem
            {
                Title = "Completed Task",
                IsComplete = true,
                DueDate = today.AddDays(1),
                CreatedAt = DateTime.UtcNow,
                SubscriptionEmails = new List<SubscriptionEmail>
                {
                    new() { Email = "user@example.com" }
                }
            };

            context.TodoItems.Add(completedTask);
            await context.SaveChangesAsync();
        }

        // Act
        await InvokeSendRemindersAsync();

        // Assert
        Assert.Empty(emailService.SentEmails);
    }

    [Fact]
    public async Task SendReminders_DoesNotSendForTasksWithoutDueDate()
    {
        // Arrange
        var emailService = (TestEmailService)_serviceProvider.GetRequiredService<IEmailService>();
        
        using (var scope = _serviceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            
            var taskWithoutDueDate = new TodoItem
            {
                Title = "Task Without Due Date",
                IsComplete = false,
                DueDate = null,
                CreatedAt = DateTime.UtcNow,
                SubscriptionEmails = new List<SubscriptionEmail>
                {
                    new() { Email = "user@example.com" }
                }
            };

            context.TodoItems.Add(taskWithoutDueDate);
            await context.SaveChangesAsync();
        }

        // Act
        await InvokeSendRemindersAsync();

        // Assert
        Assert.Empty(emailService.SentEmails);
    }

    [Fact]
    public async Task SendReminders_DoesNotSendForTasksDueBeyond3Days()
    {
        // Arrange
        var emailService = (TestEmailService)_serviceProvider.GetRequiredService<IEmailService>();
        var today = DateTime.UtcNow.Date;
        
        using (var scope = _serviceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            
            var taskDueIn4Days = new TodoItem
            {
                Title = "Task Due In 4 Days",
                IsComplete = false,
                DueDate = today.AddDays(4),
                CreatedAt = DateTime.UtcNow,
                SubscriptionEmails = new List<SubscriptionEmail>
                {
                    new() { Email = "user@example.com" }
                }
            };

            context.TodoItems.Add(taskDueIn4Days);
            await context.SaveChangesAsync();
        }

        // Act
        await InvokeSendRemindersAsync();

        // Assert
        Assert.Empty(emailService.SentEmails);
    }

    [Fact]
    public async Task SendReminders_DoesNotSendForPastDueTasks()
    {
        // Arrange
        var emailService = (TestEmailService)_serviceProvider.GetRequiredService<IEmailService>();
        var today = DateTime.UtcNow.Date;
        
        using (var scope = _serviceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            
            var pastDueTask = new TodoItem
            {
                Title = "Past Due Task",
                IsComplete = false,
                DueDate = today.AddDays(-1),
                CreatedAt = DateTime.UtcNow,
                SubscriptionEmails = new List<SubscriptionEmail>
                {
                    new() { Email = "user@example.com" }
                }
            };

            context.TodoItems.Add(pastDueTask);
            await context.SaveChangesAsync();
        }

        // Act
        await InvokeSendRemindersAsync();

        // Assert
        Assert.Empty(emailService.SentEmails);
    }

    [Fact]
    public async Task SendReminders_SendsForTaskDueToday()
    {
        // Arrange
        var emailService = (TestEmailService)_serviceProvider.GetRequiredService<IEmailService>();
        var today = DateTime.UtcNow.Date;
        
        using (var scope = _serviceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            
            var taskDueToday = new TodoItem
            {
                Title = "Task Due Today",
                IsComplete = false,
                DueDate = today,
                CreatedAt = DateTime.UtcNow,
                SubscriptionEmails = new List<SubscriptionEmail>
                {
                    new() { Email = "user@example.com" }
                }
            };

            context.TodoItems.Add(taskDueToday);
            await context.SaveChangesAsync();
        }

        // Act
        await InvokeSendRemindersAsync();

        // Assert
        Assert.Single(emailService.SentEmails);
        Assert.Equal("user@example.com", emailService.SentEmails[0].Email);
        Assert.Equal("Task Due Today", emailService.SentEmails[0].Title);
    }

    [Fact]
    public async Task SendReminders_DoesNotSendForTasksWithoutSubscribers()
    {
        // Arrange
        var emailService = (TestEmailService)_serviceProvider.GetRequiredService<IEmailService>();
        var today = DateTime.UtcNow.Date;
        
        using (var scope = _serviceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            
            var taskWithoutSubscribers = new TodoItem
            {
                Title = "Task Without Subscribers",
                IsComplete = false,
                DueDate = today.AddDays(1),
                CreatedAt = DateTime.UtcNow,
                SubscriptionEmails = new List<SubscriptionEmail>()
            };

            context.TodoItems.Add(taskWithoutSubscribers);
            await context.SaveChangesAsync();
        }

        // Act
        await InvokeSendRemindersAsync();

        // Assert
        Assert.Empty(emailService.SentEmails);
    }

    [Fact]
    public async Task SendReminders_HandlesMultipleTasksAndSubscribers()
    {
        // Arrange
        var emailService = (TestEmailService)_serviceProvider.GetRequiredService<IEmailService>();
        var today = DateTime.UtcNow.Date;
        
        using (var scope = _serviceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            
            var task1 = new TodoItem
            {
                Title = "Task 1",
                IsComplete = false,
                DueDate = today.AddDays(1),
                CreatedAt = DateTime.UtcNow,
                SubscriptionEmails = new List<SubscriptionEmail>
                {
                    new() { Email = "user1@example.com" },
                    new() { Email = "user2@example.com" }
                }
            };

            var task2 = new TodoItem
            {
                Title = "Task 2",
                IsComplete = false,
                DueDate = today.AddDays(2),
                CreatedAt = DateTime.UtcNow,
                SubscriptionEmails = new List<SubscriptionEmail>
                {
                    new() { Email = "user1@example.com" }
                }
            };

            context.TodoItems.AddRange(task1, task2);
            await context.SaveChangesAsync();
        }

        // Act
        await InvokeSendRemindersAsync();

        // Assert
        Assert.Equal(3, emailService.SentEmails.Count);
        Assert.Equal(2, emailService.SentEmails.Count(e => e.Email == "user1@example.com"));
        Assert.Single(emailService.SentEmails.Where(e => e.Email == "user2@example.com"));
    }

    private async Task InvokeSendRemindersAsync()
    {
        // Use reflection to invoke the private SendRemindersAsync method
        var service = new DailyReminderService(_serviceProvider, 
            _serviceProvider.GetRequiredService<ILogger<DailyReminderService>>());
        
        var method = typeof(DailyReminderService).GetMethod(
            "SendRemindersAsync", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (method != null)
        {
            await (Task)method.Invoke(service, new object[] { CancellationToken.None })!;
        }
    }
}

public class TestEmailService : IEmailService
{
    public List<(string Email, string Title, DateTime DueDate)> SentEmails { get; } = new();

    public Task SendReminderEmailAsync(string email, string todoTitle, DateTime dueDate)
    {
        SentEmails.Add((email, todoTitle, dueDate));
        return Task.CompletedTask;
    }
}
