namespace bookTracker.Models.DTOs;

public sealed record CreateReviewDto(
    string? DisplayName,
    int Rating,
    string? Text
);
