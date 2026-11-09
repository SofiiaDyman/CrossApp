using System.Text.Json;
using Core;
using Core.Services;
using Core.Storage;

public static class ImportCommand
{
    private sealed class ConsoleProgress(string suffix) : IProgress<int>
    {
        public void Report(int value) => Console.Write($"\r{value}{suffix}");
    }

    public static async Task<int> RunAsync(CatalogService service, string[] args)
    {
        string path = Arg(args, "--path=") ?? Path.Combine(AppContext.BaseDirectory, "data", "import.json");
        bool stream = args.Contains("--stream");
        var timeout = int.TryParse(Arg(args, "--timeout="), out var ms)
            ? TimeSpan.FromMilliseconds(ms) : Timeout.InfiniteTimeSpan;

        using var userCts = new CancellationTokenSource();
        using var timeoutCts = new CancellationTokenSource(timeout);
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(userCts.Token, timeoutCts.Token);

        ConsoleCancelEventHandler onCancel = (_, e) => { e.Cancel = true; userCts.Cancel(); };
        Console.CancelKeyPress += onCancel;

        try
        {
            var importer = new JsonCatalogImporter();
            var added = stream
                ? await service.ImportStreamAsync(importer, path, linked.Token, new ConsoleProgress(" записів"))
                : await service.ImportAsync(importer, path, linked.Token, new ConsoleProgress("%"));
            Console.WriteLine();
            Console.WriteLine($"Імпортовано записів: {added}");
            return 0;
        }
        catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested)
        { Console.WriteLine("\nІмпорт скасовано: перевищено таймаут."); return 2; }
        catch (OperationCanceledException)
        { Console.WriteLine("\nІмпорт скасовано."); return 2; }
        catch (DomainException ex)
        { Console.WriteLine($"\nДані відхилено: {ex.Message}"); return 3; }
        catch (FileNotFoundException)
        { Console.WriteLine($"\nФайл не знайдено: {path}"); return 4; }
        catch (DirectoryNotFoundException)
        { Console.WriteLine($"\nКаталог не знайдено: {path}"); return 4; }
        catch (Exception ex) when (ex is StorageException or JsonException)
        { Console.WriteLine($"\nПомилка читання: {ex.Message}"); return 5; }
        finally
        {
            Console.CancelKeyPress -= onCancel;
        }
    }

    public static async Task<int> GenerateAsync(string[] args)
    {
        int count = int.TryParse(Arg(args, "--count="), out var c) ? c : 300_000;
        string path = Path.GetFullPath(Arg(args, "--path=") ?? "data/big.json");

        var dtos = SampleData.Generate(count)
            .Select(p => p.ToDto() with { Quantity = Math.Max(1, p.Quantity) })
            .ToList();

        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        await using var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true);
        await JsonSerializer.SerializeAsync(fs, dtos, new JsonSerializerOptions { WriteIndented = true });
        Console.WriteLine($"Згенеровано {count} записів: {path}");
        return 0;
    }

    private static string? Arg(string[] args, string prefix) =>
        args.FirstOrDefault(a => a.StartsWith(prefix, StringComparison.Ordinal))?[prefix.Length..];
}
