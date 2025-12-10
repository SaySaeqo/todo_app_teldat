using Microsoft.EntityFrameworkCore;
using todo_app_teldat.Data;
using todo_app_teldat.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add email service and daily reminder background service
builder.Services.AddSingleton<IEmailService, MockEmailService>();
builder.Services.AddHostedService<DailyReminderService>();

var app = builder.Build();

// Apply pending migrations and create the database if needed.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    // Only migrate if using a relational database (not InMemory for tests)
    if (db.Database.ProviderName != "Microsoft.EntityFrameworkCore.InMemory")
    {
        db.Database.Migrate();
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapControllers();

app.Run();

// Make the implicit Program class accessible to tests
public partial class Program { }
