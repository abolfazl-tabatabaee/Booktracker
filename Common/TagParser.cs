namespace bookTracker.Common;

public static class TagParser
{
    public static string[] SplitTags(string? tagsCsv)
    {
        if (string.IsNullOrWhiteSpace(tagsCsv)) return [];
        return tagsCsv
            .Split(new[] { ',', '،' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim())
            .Where(x => x.Length > 0)
            .ToArray();
    }
}
