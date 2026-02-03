using bookTracker.Common;
using Microsoft.AspNetCore.Http;

namespace bookTracker.Storage;

public interface ICoverStorage
{
    Task<Result<string>> SaveCoverAsync(IFormFile file, CancellationToken ct = default);
    void DeleteIfLocal(string? coverPath);
}
