using Core.Services;
using Core.Storage;

public static class ExportCommand
{
    public static int Run(CatalogService service, string dataFile)
    {
        var file = new FileCatalogStore(dataFile);
        int count = file.ReplaceAll(service.All());
        Console.WriteLine($"Експортовано записів: {count}");
        Console.WriteLine($"{dataFile}:");
        foreach (var line in File.ReadLines(dataFile).Take(12)) Console.WriteLine(line);
        return 0;
    }
}
