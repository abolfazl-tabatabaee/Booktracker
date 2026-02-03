namespace bookTracker.Models.DTOs;

public sealed record BookDetailsDto(
    int Id,
    string Title,
    string Author,
    int Year,
    string Genre,
    string Lang,
    string[] Tags,
    string Description,
    string Cover,
    List<ReviewDto> ReviewList
);
