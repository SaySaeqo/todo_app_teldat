using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using todo_app_teldat.Controllers;
using todo_app_teldat.Models;
using Xunit;

namespace backend.Tests;

public partial class TodoItemsControllerTests : IDisposable
{
    private readonly HttpClient _client;
    private readonly TestWebApplicationFactory<Program> _factory;

    public TodoItemsControllerTests()
    {
        _factory = new TestWebApplicationFactory<Program>();
        _client = _factory.CreateClient();
    }

    public void Dispose()
    {
        _client?.Dispose();
        _factory?.Dispose();
    }

    [Fact]
    public async Task GetAll_ReturnsEmptyList_WhenNoItems()
    {
        // Act
        var response = await _client.GetAsync("/api/todoitems");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<PagedResult<TodoItem>>();
        Assert.NotNull(result);
        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalCount);
    }

    [Fact]
    public async Task GetAll_ReturnsPaginatedItems_WithDefaultPaging()
    {
        // Arrange
        var items = Enumerable.Range(1, 25).Select(i => new TodoItem
        {
            Title = $"Task {i}",
            IsComplete = false
        }).ToList();

        foreach (var item in items)
        {
            await _client.PostAsJsonAsync("/api/todoitems", item);
        }

        // Act
        var response = await _client.GetAsync("/api/todoitems");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<PagedResult<TodoItem>>();
        Assert.NotNull(result);
        Assert.Equal(20, result.Items.Count); // Default page size
        Assert.Equal(25, result.TotalCount);
        Assert.Equal(1, result.Page);
    }

    [Fact]
    public async Task GetAll_ReturnsFilteredItems_ByIsComplete()
    {
        // Arrange
        await _client.PostAsJsonAsync("/api/todoitems", new TodoItem { Title = "Complete Task", IsComplete = true });
        await _client.PostAsJsonAsync("/api/todoitems", new TodoItem { Title = "Incomplete Task", IsComplete = false });

        // Act
        var response = await _client.GetAsync("/api/todoitems?isComplete=true");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<PagedResult<TodoItem>>();
        Assert.NotNull(result);
        Assert.Single(result.Items);
        Assert.True(result.Items[0].IsComplete);
        Assert.Equal("Complete Task", result.Items[0].Title);
    }

    [Fact]
    public async Task GetAll_ReturnsFilteredItems_BySearch()
    {
        // Arrange
        await _client.PostAsJsonAsync("/api/todoitems", new TodoItem { Title = "Buy groceries", IsComplete = false });
        await _client.PostAsJsonAsync("/api/todoitems", new TodoItem { Title = "Read book", IsComplete = false });

        // Act
        var response = await _client.GetAsync("/api/todoitems?search=groceries");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<PagedResult<TodoItem>>();
        Assert.NotNull(result);
        Assert.Single(result.Items);
        Assert.Equal("Buy groceries", result.Items[0].Title);
    }

    [Fact]
    public async Task GetAll_ReturnsFilteredItems_ByDueDate()
    {
        // Arrange
        var tomorrow = DateTime.UtcNow.AddDays(1);
        var nextWeek = DateTime.UtcNow.AddDays(7);
        
        await _client.PostAsJsonAsync("/api/todoitems", new TodoItem { Title = "Task 1", DueDate = tomorrow });
        await _client.PostAsJsonAsync("/api/todoitems", new TodoItem { Title = "Task 2", DueDate = nextWeek });

        // Act
        var response = await _client.GetAsync($"/api/todoitems?dueBefore={nextWeek.AddDays(-1):O}");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<PagedResult<TodoItem>>();
        Assert.NotNull(result);
        Assert.Single(result.Items);
        Assert.Equal("Task 1", result.Items[0].Title);
    }

    [Theory]
    [InlineData("createdDesc")]
    [InlineData("createdAsc")]
    [InlineData("titleAsc")]
    [InlineData("titleDesc")]
    public async Task GetAll_ReturnsSortedItems_ByDifferentCriteria(string sortBy)
    {
        // Arrange
        await _client.PostAsJsonAsync("/api/todoitems", new TodoItem { Title = "Zebra task" });
        await _client.PostAsJsonAsync("/api/todoitems", new TodoItem { Title = "Alpha task" });

        // Act
        var response = await _client.GetAsync($"/api/todoitems?sortBy={sortBy}");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<PagedResult<TodoItem>>();
        Assert.NotNull(result);
        Assert.Equal(2, result.Items.Count);
        
        if (sortBy == "titleAsc")
        {
            Assert.Equal("Alpha task", result.Items[0].Title);
            Assert.Equal("Zebra task", result.Items[1].Title);
        }
        else if (sortBy == "titleDesc")
        {
            Assert.Equal("Zebra task", result.Items[0].Title);
            Assert.Equal("Alpha task", result.Items[1].Title);
        }
    }

    [Fact]
    public async Task GetAll_ReturnsCustomPageSize()
    {
        // Arrange
        for (int i = 0; i < 10; i++)
        {
            await _client.PostAsJsonAsync("/api/todoitems", new TodoItem { Title = $"Task {i}" });
        }

        // Act
        var response = await _client.GetAsync("/api/todoitems?pageSize=5");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<PagedResult<TodoItem>>();
        Assert.NotNull(result);
        Assert.Equal(5, result.Items.Count);
        Assert.Equal(10, result.TotalCount);
    }
}
