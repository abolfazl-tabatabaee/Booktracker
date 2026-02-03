namespace bookTracker.Models.DTOs;

public sealed record BookListDto(
    int Id,
    string Title,
    string Author,
    int Year,
    string Genre,
    string Lang,
    string[] Tags,
    string Cover,
    int ReviewCount,
    double AvgRating
);
