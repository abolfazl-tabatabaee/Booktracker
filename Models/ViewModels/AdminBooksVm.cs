using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace bookTracker.Models.ViewModels;

public class AdminBookListItemVm
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Author { get; set; } = "";
    public int Year { get; set; }
    public string Genre { get; set; } = "";
    public int ReviewCount { get; set; }
    public double AvgRating { get; set; }
}

public class AdminBooksIndexVm
{
    public string? Q { get; set; }
    public List<AdminBookListItemVm> Items { get; set; } = new();
}

public class AdminBookEditVm
{
    public int? Id { get; set; }

    [Required, MaxLength(200)]
    public string Title { get; set; } = "";

    [Required, MaxLength(150)]
    public string Author { get; set; } = "";

    [Required, MaxLength(80)]
    public string Genre { get; set; } = "";

    [Range(0, 2100)]
    public int Year { get; set; }

    [MaxLength(10)]
    public string Lang { get; set; } = "fa";

    [MaxLength(500)]
    public string? CoverPath { get; set; }

    public IFormFile? CoverFile { get; set; }

    [MaxLength(2000)]
    public string? Description { get; set; }

    [MaxLength(600)]
    public string? TagsCsv { get; set; }
}
