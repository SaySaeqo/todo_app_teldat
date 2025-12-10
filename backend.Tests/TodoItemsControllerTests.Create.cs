using System.Net;
using System.Net.Http.Json;
using todo_app_teldat.Models;
using Xunit;

namespace backend.Tests;

public partial class TodoItemsControllerTests
{
    [Fact]
    public async Task Create_CreatesNewItem_AndReturnsCreated()
    {
        // Arrange
        var newItem = new TodoItem
        {
            Title = "New Task",
            IsComplete = false,
            DueDate = DateTime.UtcNow.AddDays(7)
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/todoitems", newItem);
        
        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<TodoItem>();
        Assert.NotNull(created);
        Assert.True(created.Id > 0);
        Assert.Equal("New Task", created.Title);
        Assert.False(created.IsComplete);
        Assert.NotEqual(default, created.CreatedAt);
    }

    [Fact]
    public async Task Create_CreatesItemWithSubscriptions()
    {
        // Arrange
        var newItem = new TodoItem
        {
            Title = "Task with subscribers",
            IsComplete = false,
            SubscriptionEmails = new List<SubscriptionEmail>
            {
                new() { Email = "user1@test.com" },
                new() { Email = "user2@test.com" }
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/todoitems", newItem);
        
        // Assert
        response.EnsureSuccessStatusCode();
        var created = await response.Content.ReadFromJsonAsync<TodoItem>();
        Assert.NotNull(created);
        Assert.Equal(2, created.SubscriptionEmails.Count);
    }

    [Fact]
    public async Task Create_ReturnsLocationHeader()
    {
        // Arrange
        var newItem = new TodoItem { Title = "Test", IsComplete = false };

        // Act
        var response = await _client.PostAsJsonAsync("/api/todoitems", newItem);
        
        // Assert
        response.EnsureSuccessStatusCode();
        Assert.NotNull(response.Headers.Location);
        Assert.Contains("/api/", response.Headers.Location.ToString().ToLower());
    }

    [Fact]
    public async Task Create_SetsCreatedAtToUtcNow()
    {
        // Arrange
        var before = DateTime.UtcNow.AddSeconds(-1);
        var newItem = new TodoItem { Title = "Test", IsComplete = false };

        // Act
        var response = await _client.PostAsJsonAsync("/api/todoitems", newItem);
        var after = DateTime.UtcNow.AddSeconds(1);
        
        // Assert
        response.EnsureSuccessStatusCode();
        var created = await response.Content.ReadFromJsonAsync<TodoItem>();
        Assert.NotNull(created);
        Assert.InRange(created.CreatedAt, before, after);
    }
}
