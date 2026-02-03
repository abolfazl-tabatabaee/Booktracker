using System.ComponentModel.DataAnnotations;

namespace bookTracker.Models.Entities;

public class Review
{
    public int Id { get; set; }

    public int BookId { get; set; }
    public Book Book { get; set; } = null!;

    public string? UserId { get; set; }
    public AppUser? User { get; set; }

    [Required, MaxLength(60)]
    public string DisplayName { get; set; } = "کاربر";

    [Range(1, 5)]
    public int Rating { get; set; }

    [Required, MaxLength(400)]
    public string Text { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
