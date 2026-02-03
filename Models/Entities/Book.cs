using System.ComponentModel.DataAnnotations;

namespace bookTracker.Models.Entities;

public class Book
{
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required, MaxLength(150)]
    public string Author { get; set; } = string.Empty;

    public int Year { get; set; }

    [MaxLength(50)]
    public string Genre { get; set; } = string.Empty;

    [MaxLength(10)]
    public string Lang { get; set; } = "fa";

    [MaxLength(500)]
    public string? CoverPath { get; set; }

    [MaxLength(2000)]
    public string? Description { get; set; }

    [MaxLength(600)]
    public string? TagsCsv { get; set; }


    public ICollection<Review> Reviews { get; set; } = new List<Review>();
}
