using Core.Abstractions;
using Core.Domain;

namespace Core.Tests;

internal sealed class ListImporter(IReadOnlyList<Product> items) : ICatalogImporter
{
    public Task<IReadOnlyList<Product>> LoadAsync(string path, CancellationToken ct = default) =>
        Task.FromResult(items);
    public IAsyncEnumerable<Product> StreamAsync(string path, CancellationToken ct = default) =>
        throw new NotSupportedException();
}

internal sealed class HangingImporter : ICatalogImporter   // «висить», доки не скасують токен
{
    public async Task<IReadOnlyList<Product>> LoadAsync(string path, CancellationToken ct = default)
    {
        await Task.Delay(Timeout.Infinite, ct);
        return [];
    }
    public IAsyncEnumerable<Product> StreamAsync(string path, CancellationToken ct = default) =>
        throw new NotSupportedException();
}

internal sealed class ListProgress : IProgress<int>
{
    public List<int> Values { get; } = [];
    public void Report(int value) => Values.Add(value);
}

internal static class TempFile
{
    public static async Task WithAsync(string json, Func<string, Task> body)
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.json");
        await File.WriteAllTextAsync(path, json);
        try { await body(path); }
        finally { File.Delete(path); }
    }
}