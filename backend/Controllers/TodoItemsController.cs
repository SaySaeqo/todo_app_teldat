using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using todo_app_teldat.Data;
using todo_app_teldat.Models;

namespace todo_app_teldat.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TodoItemsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public TodoItemsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<TodoItem>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] bool? isComplete = null,
        [FromQuery] string? search = null,
        [FromQuery] DateTime? dueAfter = null,
        [FromQuery] DateTime? dueBefore = null,
        [FromQuery] string? sortBy = "createdDesc")
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = _context.TodoItems
            .Include(t => t.SubscriptionEmails)
            .AsQueryable();

        if (isComplete.HasValue)
        {
            query = query.Where(t => t.IsComplete == isComplete.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = $"%{search.Trim()}%";
            query = query.Where(t => EF.Functions.Like(t.Title, term));
        }

        if (dueAfter.HasValue)
        {
            query = query.Where(t => t.DueDate >= dueAfter.Value);
        }

        if (dueBefore.HasValue)
        {
            query = query.Where(t => t.DueDate <= dueBefore.Value);
        }

        var totalCount = await query.CountAsync();

        query = sortBy?.ToLower() switch
        {
            "createdasc" => query.OrderBy(t => t.CreatedAt),
            "duedesc" => query.OrderByDescending(t => t.DueDate == null ? DateTime.MaxValue : t.DueDate).ThenByDescending(t => t.CreatedAt),
            "dueasc" => query.OrderBy(t => t.DueDate == null ? DateTime.MaxValue : t.DueDate).ThenByDescending(t => t.CreatedAt),
            "titleasc" => query.OrderBy(t => t.Title),
            "titledesc" => query.OrderByDescending(t => t.Title),
            _ => query.OrderByDescending(t => t.CreatedAt)
        };

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<TodoItem>(items, totalCount, page, pageSize);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TodoItem>> GetById(int id)
    {
        var item = await _context.TodoItems
            .Include(t => t.SubscriptionEmails)
            .FirstOrDefaultAsync(t => t.Id == id);
        if (item == null)
        {
            return NotFound();
        }

        return item;
    }

    [HttpPost]
    public async Task<ActionResult<TodoItem>> Create([FromBody] TodoItem item)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        item.Id = 0;
        item.CreatedAt = DateTime.UtcNow;

        _context.TodoItems.Add(item);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = item.Id }, item);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] TodoItem item)
    {
        if (id != item.Id)
        {
            return BadRequest("Id in path and body must match.");
        }

        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var existing = await _context.TodoItems
            .Include(t => t.SubscriptionEmails)
            .FirstOrDefaultAsync(t => t.Id == id);
        if (existing == null)
        {
            return NotFound();
        }

        existing.Title = item.Title;
        existing.IsComplete = item.IsComplete;
        existing.DueDate = item.DueDate;

        existing.SubscriptionEmails.Clear();
        if (item.SubscriptionEmails != null && item.SubscriptionEmails.Count > 0)
        {
            foreach (var sub in item.SubscriptionEmails)
            {
                existing.SubscriptionEmails.Add(new SubscriptionEmail
                {
                    Email = sub.Email,
                    TodoItemId = existing.Id
                });
            }
        }

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var existing = await _context.TodoItems.FindAsync(id);
        if (existing == null)
        {
            return NotFound();
        }

        _context.TodoItems.Remove(existing);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}

public record PagedResult<T>(IReadOnlyList<T> Items, int TotalCount, int Page, int PageSize);
