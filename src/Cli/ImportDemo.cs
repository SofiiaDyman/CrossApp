using Core.Import;

public static class ImportDemo
{
    public static int Run(string importPath)
    {
        if (!File.Exists(importPath))
        {
            Console.WriteLine($"Файл не знайдено: {Path.GetFullPath(importPath)}");
            return 1;
        }

        int skipped = 0;
        foreach (string line in ImportDispatcher.Load(importPath, msg =>
        {
            skipped++;
            Console.WriteLine($"  [Action] пропущено: {msg}");
        }))
            Console.WriteLine(line);
        Console.WriteLine($"Action<string> спрацював разів: {skipped}");

        DomainDemo.ImportToDomain(importPath);
        DomainDemo.Run();
        return 0;
    }
}
