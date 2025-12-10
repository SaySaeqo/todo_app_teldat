using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using todo_app_teldat.Data;

namespace backend.Tests;

public class TestWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the existing DbContext registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Add in-memory database for testing with unique name per instance
            var databaseName = $"TestDatabase_{Guid.NewGuid()}";
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseInMemoryDatabase(databaseName);
            });

            // Build the service provider
            var sp = services.BuildServiceProvider();

            // Create a scope to obtain a reference to the database context
            using var scope = sp.CreateScope();
            var scopedServices = scope.ServiceProvider;
            var db = scopedServices.GetRequiredService<ApplicationDbContext>();

            // Ensure the database is created
            db.Database.EnsureCreated();
        });
    }
}
