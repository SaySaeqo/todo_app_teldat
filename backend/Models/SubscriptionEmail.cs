using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace todo_app_teldat.Models;

public class SubscriptionEmail
{
    public int Id { get; set; }

    [Required]
    [EmailAddress]
    [MaxLength(320)]
    public string Email { get; set; } = string.Empty;

    public int TodoItemId { get; set; }

    [JsonIgnore]
    public TodoItem? TodoItem { get; set; }
}
