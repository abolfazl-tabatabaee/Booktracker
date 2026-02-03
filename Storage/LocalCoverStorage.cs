using bookTracker.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace bookTracker.Storage;

public sealed class LocalCoverStorage : ICoverStorage
{
    private readonly IWebHostEnvironment _env;

    private const long MaxCoverBytes = 1 * 1024 * 1024;
    private static readonly HashSet<string> AllowedExt = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp"
    };

    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg",
        "image/png",
        "image/webp"
    };

    public LocalCoverStorage(IWebHostEnvironment env) => _env = env;

    public async Task<Result<string>> SaveCoverAsync(IFormFile file, CancellationToken ct = default)
    {
        if (file is null || file.Length <= 0)
            return Result<string>.Validation(new Dictionary<string, string[]>
            {
                ["CoverFile"] = ["فایل کاور نامعتبر است."]
            });

        if (file.Length > MaxCoverBytes)
            return Result<string>.Validation(new Dictionary<string, string[]>
            {
                ["CoverFile"] = ["حجم کاور باید حداکثر 1MB باشد."]
            });

        var ext = Path.GetExtension(file.FileName ?? "");
        if (string.IsNullOrWhiteSpace(ext) || !AllowedExt.Contains(ext))
            return Result<string>.Validation(new Dictionary<string, string[]>
            {
                ["CoverFile"] = ["فقط فایل‌های jpg/png/webp مجاز هستند."]
            });

        var contentType = (file.ContentType ?? "").Trim();
        if (string.IsNullOrWhiteSpace(contentType) || !AllowedContentTypes.Contains(contentType))
            return Result<string>.Validation(new Dictionary<string, string[]>
            {
                ["CoverFile"] = ["نوع فایل معتبر نیست."]
            });

        var signatureOk = await IsValidImageSignatureAsync(file, ext, ct);
        if (!signatureOk)
            return Result<string>.Validation(new Dictionary<string, string[]>
            {
                ["CoverFile"] = ["فایل تصویر معتبر نیست (signature نامعتبر)."]
            });

        var uploadsDir = Path.Combine(_env.WebRootPath, "uploads", "covers");
        Directory.CreateDirectory(uploadsDir);

        var safeExt = ext.ToLowerInvariant();
        var name = $"{Guid.NewGuid():N}{safeExt}";
        var absPath = Path.Combine(uploadsDir, name);

        await using (var fs = new FileStream(absPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
        {
            await file.CopyToAsync(fs, ct);
        }

        var relPath = $"/uploads/covers/{name}";
        return Result<string>.Ok(relPath);
    }

    public void DeleteIfLocal(string? coverPath)
    {
        if (string.IsNullOrWhiteSpace(coverPath)) return;

        if (!coverPath.StartsWith("/uploads/covers/", StringComparison.OrdinalIgnoreCase)) return;

        var fileName = Path.GetFileName(coverPath);
        if (string.IsNullOrWhiteSpace(fileName)) return;

        var ext = Path.GetExtension(fileName);
        if (string.IsNullOrWhiteSpace(ext) || !AllowedExt.Contains(ext)) return;

        var abs = Path.Combine(_env.WebRootPath, "uploads", "covers", fileName);

        var root = Path.GetFullPath(Path.Combine(_env.WebRootPath, "uploads", "covers") + Path.DirectorySeparatorChar);
        var full = Path.GetFullPath(abs);

        if (!full.StartsWith(root, StringComparison.OrdinalIgnoreCase))
            return;

        if (File.Exists(full))
        {
            try { File.Delete(full); }
            catch { /* ignore */ }
        }
    }

    private static async Task<bool> IsValidImageSignatureAsync(IFormFile file, string ext, CancellationToken ct)
    {
        var buffer = new byte[16];

        await using var stream = file.OpenReadStream();
        var read = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length), ct);
        if (read < 12) return false;

        if (ext.Equals(".jpg", StringComparison.OrdinalIgnoreCase) || ext.Equals(".jpeg", StringComparison.OrdinalIgnoreCase))
        {
            return buffer[0] == 0xFF && buffer[1] == 0xD8 && buffer[2] == 0xFF;
        }

        if (ext.Equals(".png", StringComparison.OrdinalIgnoreCase))
        {
            return buffer[0] == 0x89 &&
                   buffer[1] == 0x50 &&
                   buffer[2] == 0x4E &&
                   buffer[3] == 0x47 &&
                   buffer[4] == 0x0D &&
                   buffer[5] == 0x0A &&
                   buffer[6] == 0x1A &&
                   buffer[7] == 0x0A;
        }

        if (ext.Equals(".webp", StringComparison.OrdinalIgnoreCase))
        {
            return buffer[0] == (byte)'R' &&
                   buffer[1] == (byte)'I' &&
                   buffer[2] == (byte)'F' &&
                   buffer[3] == (byte)'F' &&
                   buffer[8] == (byte)'W' &&
                   buffer[9] == (byte)'E' &&
                   buffer[10] == (byte)'B' &&
                   buffer[11] == (byte)'P';
        }

        return false;
    }
}
