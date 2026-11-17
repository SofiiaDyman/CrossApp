using Core;
using Core.Abstractions;
using Core.Domain;
using Core.Reports;
using Core.Services;
using Core.Storage;

try
{
    var settings = AppConfig.Load(args);
    string command = args.FirstOrDefault(a => !a.StartsWith("--")) ?? "report-top";

    ICatalogStore store = StoreFactory.Create(args, settings.DataFile);
    var service = new CatalogService(store);
    var items = service.All();

    switch (command)
    {
        case "report-top":
            ReportPrinter.Print(ReportService.TopByQuantity(items, 5));
            break;
        case "report-low":
            ReportPrinter.Print(ReportService.LowStock(items, 10));
            break;
        case "report-groups":
            ReportPrinter.Print(ReportService.ByUnit(items));
            break;
        case "report-top-groups":
            ReportPrinter.Print(ReportService.TopUnits(items, 2));
            break;
        case "report-groups-query":
            var viaQuery = ReportService.ByUnitQuerySyntax(items);
            ReportPrinter.Print(viaQuery);
            Console.WriteLine($" Збігається з методним синтаксисом: {viaQuery.SequenceEqual(ReportService.ByUnit(items))}");
            break;
        case "report-sum":
            ReportPrinter.Print(ReportService.Totals(items));
            break;
        case "report-moves":
            ReportPrinter.Print(ReportService.Movements(items, SampleData.Movements()));
            break;
        case "report-moves-sum":
            ReportPrinter.Print(ReportService.MovementSummaries(items, SampleData.Movements()));
            break;
        case "report-empty":
            IReadOnlyList<Product> empty = [];
            ReportPrinter.Print(ReportService.LowStock(empty, 10));
            ReportPrinter.Print(ReportService.TopByQuantity(empty, 5));
            ReportPrinter.Print(ReportService.ByUnit(empty));
            ReportPrinter.Print(ReportService.Totals(empty));
            break;
        case "report-lazy":
            LazyDemo.Run(service);
            break;
        case "report-bench":
            BenchDemo.Run();
            break;
        case "config":
            AppConfig.PrintSummary(settings);
            break;
        case "export":
            AppConfig.PrintSummary(settings);
            return ExportCommand.Run(service, settings.DataFile);
        case "import":
            AppConfig.PrintSummary(settings);
            return await ImportCommand.RunAsync(service, args);
        case "add":
        {
            string unit = AppConfig.Arg(args, "--unit=") ?? settings.Options.Warehouse.DefaultUnit;
            var p = service.Add(
                AppConfig.Arg(args, "--sku=") ?? "SKU-NEW",
                AppConfig.Arg(args, "--name=") ?? "Новий товар",
                AppConfig.Arg(args, "--category=") ?? "магніт",
                unit,
                int.TryParse(AppConfig.Arg(args, "--qty="), out var q) ? q : 1);
            Console.WriteLine($"Додано: {p}");
            break;
        }
        case "import-gen":
            return await ImportCommand.GenerateAsync(args);
        case "demo-events":
            Console.WriteLine($"Сховище: {store.GetType().Name}");
            EventDemo.Run(service);
            SinkDemo.Run();
            break;
        default:
            if (Path.HasExtension(command))
                return ImportDemo.Run(command);
            Console.WriteLine("report-top | report-low | report-groups | report-top-groups | report-groups-query |");
            Console.WriteLine("report-sum | report-moves | report-moves-sum | report-empty | report-lazy | report-bench | config | export | import | import-gen | demo-events | add");
            return 1;
    }

    if (store is CachingCatalogStore cache && items.Count > 0)
    {
        string id = items[0].Id;
        int c0 = cache.InnerCalls;
        service.Find(id);
        int c1 = cache.InnerCalls;
        service.Find(id);
        Console.WriteLine($"Звернень до внутрішнього сховища: {c0} → {c1} → {cache.InnerCalls}");
    }

    return 0;
}
catch (FileNotFoundException ex)
{
    Console.WriteLine($"Файл не знайдено: {ex.FileName ?? ex.Message}");
    return 4;
}
catch (StorageException ex)
{
    Console.WriteLine($"Помилка сховища: {ex.Message}");
    return 5;
}
catch (DomainException ex)
{
    Console.WriteLine($"Дані відхилено: {ex.Message}");
    return 3;
}
catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
{
    Console.WriteLine($"Помилка доступу до файлу: {ex.Message}");
    return 4;
}
