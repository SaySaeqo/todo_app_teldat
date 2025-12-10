using System.Net;
using System.Net.Http.Json;
using todo_app_teldat.Models;
using Xunit;

namespace backend.Tests;

public partial class TodoItemsControllerTests
{
    [Fact]
    public async Task Update_UpdatesExistingItem_AndReturnsNoContent()
    {
        // Arrange
        var createResponse = await _client.PostAsJsonAsync("/api/todoitems", new TodoItem
        {
            Title = "Original Title",
            IsComplete = false
        });
        var created = await createResponse.Content.ReadFromJsonAsync<TodoItem>();

        var updatedItem = new TodoItem
        {
            Id = created!.Id,
            Title = "Updated Title",
            IsComplete = true,
            DueDate = DateTime.UtcNow.AddDays(5)
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/todoitems/{created.Id}", updatedItem);
        
        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Verify the update
        var getResponse = await _client.GetAsync($"/api/todoitems/{created.Id}");
        var updated = await getResponse.Content.ReadFromJsonAsync<TodoItem>();
        Assert.NotNull(updated);
        Assert.Equal("Updated Title", updated.Title);
        Assert.True(updated.IsComplete);
    }

    [Fact]
    public async Task Update_ReturnsNotFound_WhenItemDoesNotExist()
    {
        // Arrange
        var item = new TodoItem
        {
            Id = 99999,
            Title = "Non-existent",
            IsComplete = false
        };

        // Act
        var response = await _client.PutAsJsonAsync("/api/todoitems/99999", item);
        
        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Update_ReturnsBadRequest_WhenIdMismatch()
    {
        // Arrange
        var createResponse = await _client.PostAsJsonAsync("/api/todoitems", new TodoItem
        {
            Title = "Test",
            IsComplete = false
        });
        var created = await createResponse.Content.ReadFromJsonAsync<TodoItem>();

        var updateItem = new TodoItem
        {
            Id = created!.Id + 1, // Different ID
            Title = "Updated",
            IsComplete = true
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/todoitems/{created.Id}", updateItem);
        
        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Update_UpdatesSubscriptionEmails()
    {
        // Arrange
        var createResponse = await _client.PostAsJsonAsync("/api/todoitems", new TodoItem
        {
            Title = "Task",
            IsComplete = false,
            SubscriptionEmails = new List<SubscriptionEmail>
            {
                new() { Email = "old@test.com" }
            }
        });
        var created = await createResponse.Content.ReadFromJsonAsync<TodoItem>();

        var updatedItem = new TodoItem
        {
            Id = created!.Id,
            Title = "Task",
            IsComplete = false,
            SubscriptionEmails = new List<SubscriptionEmail>
            {
                new() { Email = "new1@test.com" },
                new() { Email = "new2@test.com" }
            }
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/todoitems/{created.Id}", updatedItem);
        
        // Assert
        response.EnsureSuccessStatusCode();

        // Verify subscriptions were updated
        var getResponse = await _client.GetAsync($"/api/todoitems/{created.Id}");
        var updated = await getResponse.Content.ReadFromJsonAsync<TodoItem>();
        Assert.NotNull(updated);
        Assert.Equal(2, updated.SubscriptionEmails.Count);
        Assert.DoesNotContain(updated.SubscriptionEmails, s => s.Email == "old@test.com");
        Assert.Contains(updated.SubscriptionEmails, s => s.Email == "new1@test.com");
    }

    [Fact]
    public async Task Update_CanClearSubscriptionEmails()
    {
        // Arrange
        var createResponse = await _client.PostAsJsonAsync("/api/todoitems", new TodoItem
        {
            Title = "Task",
            IsComplete = false,
            SubscriptionEmails = new List<SubscriptionEmail>
            {
                new() { Email = "test@test.com" }
            }
        });
        var created = await createResponse.Content.ReadFromJsonAsync<TodoItem>();

        var updatedItem = new TodoItem
        {
            Id = created!.Id,
            Title = "Task",
            IsComplete = false,
            SubscriptionEmails = new List<SubscriptionEmail>()
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/todoitems/{created.Id}", updatedItem);
        
        // Assert
        response.EnsureSuccessStatusCode();

        var getResponse = await _client.GetAsync($"/api/todoitems/{created.Id}");
        var updated = await getResponse.Content.ReadFromJsonAsync<TodoItem>();
        Assert.NotNull(updated);
        Assert.Empty(updated.SubscriptionEmails);
    }
}
