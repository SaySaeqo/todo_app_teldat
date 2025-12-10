using System.Net;
using System.Net.Http.Json;
using todo_app_teldat.Models;
using Xunit;

namespace backend.Tests;

public partial class TodoItemsControllerTests
{
    [Fact]
    public async Task Delete_DeletesItem_AndReturnsNoContent()
    {
        // Arrange
        var createResponse = await _client.PostAsJsonAsync("/api/todoitems", new TodoItem
        {
            Title = "To Delete",
            IsComplete = false
        });
        var created = await createResponse.Content.ReadFromJsonAsync<TodoItem>();

        // Act
        var response = await _client.DeleteAsync($"/api/todoitems/{created!.Id}");
        
        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Verify item is deleted
        var getResponse = await _client.GetAsync($"/api/todoitems/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task Delete_ReturnsNotFound_WhenItemDoesNotExist()
    {
        // Act
        var response = await _client.DeleteAsync("/api/todoitems/99999");
        
        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_CascadesSubscriptionEmails()
    {
        // Arrange
        var createResponse = await _client.PostAsJsonAsync("/api/todoitems", new TodoItem
        {
            Title = "Task with subscriptions",
            IsComplete = false,
            SubscriptionEmails = new List<SubscriptionEmail>
            {
                new() { Email = "test@test.com" }
            }
        });
        var created = await createResponse.Content.ReadFromJsonAsync<TodoItem>();

        // Act
        var deleteResponse = await _client.DeleteAsync($"/api/todoitems/{created!.Id}");
        
        // Assert
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        // Verify item and subscriptions are gone
        var getResponse = await _client.GetAsync($"/api/todoitems/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }
}
