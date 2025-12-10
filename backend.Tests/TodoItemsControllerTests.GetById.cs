using System.Net;
using System.Net.Http.Json;
using todo_app_teldat.Models;
using Xunit;

namespace backend.Tests;

public partial class TodoItemsControllerTests
{
    [Fact]
    public async Task GetById_ReturnsItem_WhenExists()
    {
        // Arrange
        var createResponse = await _client.PostAsJsonAsync("/api/todoitems", new TodoItem
        {
            Title = "Test Task",
            IsComplete = false
        });
        var created = await createResponse.Content.ReadFromJsonAsync<TodoItem>();

        // Act
        var response = await _client.GetAsync($"/api/todoitems/{created!.Id}");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<TodoItem>();
        Assert.NotNull(result);
        Assert.Equal(created.Id, result.Id);
        Assert.Equal("Test Task", result.Title);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenDoesNotExist()
    {
        // Act
        var response = await _client.GetAsync("/api/todoitems/99999");
        
        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetById_IncludesSubscriptionEmails()
    {
        // Arrange
        var createResponse = await _client.PostAsJsonAsync("/api/todoitems", new TodoItem
        {
            Title = "Task with subscriptions",
            IsComplete = false,
            SubscriptionEmails = new List<SubscriptionEmail>
            {
                new() { Email = "test1@example.com" },
                new() { Email = "test2@example.com" }
            }
        });
        var created = await createResponse.Content.ReadFromJsonAsync<TodoItem>();

        // Act
        var response = await _client.GetAsync($"/api/todoitems/{created!.Id}");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<TodoItem>();
        Assert.NotNull(result);
        Assert.Equal(2, result.SubscriptionEmails.Count);
        Assert.Contains(result.SubscriptionEmails, s => s.Email == "test1@example.com");
        Assert.Contains(result.SubscriptionEmails, s => s.Email == "test2@example.com");
    }
}
