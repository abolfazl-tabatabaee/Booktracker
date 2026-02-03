namespace bookTracker.Models.DTOs;

public sealed record ReviewDto(
    int Id,
    string Name,
    int Rating,
    string Text,
    long CreatedAt
);
