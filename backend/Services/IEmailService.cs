namespace todo_app_teldat.Services;

public interface IEmailService
{
    Task SendReminderEmailAsync(string email, string todoTitle, DateTime dueDate);
}
