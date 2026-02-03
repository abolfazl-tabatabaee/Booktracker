namespace bookTracker.Common;

public enum ResultKind
{
    Ok = 0,
    NotFound = 1,
    Unauthorized = 2,
    Validation = 3,
    Error = 4
}

public sealed record Result(
    ResultKind Kind,
    string? Error = null,
    IReadOnlyDictionary<string, string[]>? ValidationErrors = null)
{
    public bool Success => Kind == ResultKind.Ok;

    public static Result Ok() => new(ResultKind.Ok);

    public static Result NotFound(string? error = null) => new(ResultKind.NotFound, error);

    public static Result Unauthorized(string? error = null) => new(ResultKind.Unauthorized, error);

    public static Result Validation(IDictionary<string, string[]> errors, string? error = null)
        => new(ResultKind.Validation, error ?? "Validation error", new Dictionary<string, string[]>(errors));

    public static Result Fail(string error) => new(ResultKind.Error, error);
}

public sealed record Result<T>(
    ResultKind Kind,
    T? Value = default,
    string? Error = null,
    IReadOnlyDictionary<string, string[]>? ValidationErrors = null)
{
    public bool Success => Kind == ResultKind.Ok;

    public static Result<T> Ok(T value) => new(ResultKind.Ok, value);

    public static Result<T> NotFound(string? error = null) => new(ResultKind.NotFound, default, error);

    public static Result<T> Unauthorized(string? error = null) => new(ResultKind.Unauthorized, default, error);

    public static Result<T> Validation(IDictionary<string, string[]> errors, string? error = null)
        => new(ResultKind.Validation, default, error ?? "Validation error", new Dictionary<string, string[]>(errors));

    public static Result<T> Fail(string error) => new(ResultKind.Error, default, error);
}
