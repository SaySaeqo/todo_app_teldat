using Microsoft.EntityFrameworkCore;
using todo_app_teldat.Models;

namespace todo_app_teldat.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<TodoItem> TodoItems => Set<TodoItem>();

    public DbSet<SubscriptionEmail> SubscriptionEmails => Set<SubscriptionEmail>();
}
