using Core.Services;
using Core.Storage;

public static class SinkDemo
{
    public static void Run()
    {
        Console.WriteLine();
        Console.WriteLine("=== Додаткове 1: INotificationSink через конструктор ===");

        var logPath = Path.Combine(AppContext.BaseDirectory, "logs", "sink.log");
        var service = new CatalogService(
            new InMemoryCatalogStore(),
            [new ConsoleSink(), new FileSink(logPath)]);

        var p = service.Add("SKU-201", "Магніт Ратуша", "магніт", "шт", 10);
        service.Issue(p.Id, 3);
    }
}