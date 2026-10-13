using Core;
using Core.Abstractions;
using Core.Import;
using Core.Services;
using Core.Storage;

bool useFile = args.Contains("--file");
bool useCache = args.Contains("--cache");
string? importPath = args.FirstOrDefault(a => !a.StartsWith("--"));

string dataPath = Path.Combine(AppContext.BaseDirectory, "data", "catalog.json");

ICatalogStore store = useFile
    ? new FileCatalogStore(dataPath)
    : new InMemoryCatalogStore(SampleData.Products());

if (useCache)
    store = new CachingCatalogStore(store);     // декоратор (додаткове 1)

var service = new CatalogService(store);

Console.WriteLine($"Сховище: {store.GetType().Name}");
StoreDemo.Run(service);

// демонстрація кешу (додаткове 1)
if (store is CachingCatalogStore cache)
{
    string id = service.All()[0].Id;
    int c0 = cache.InnerCalls;
    service.Find(id);
    int c1 = cache.InnerCalls;
    service.Find(id);
    Console.WriteLine($"Звернень до внутрішнього сховища: {c0} → {c1} → {cache.InnerCalls}");
}

// старі демо з тижнів 3-4, лише якщо явно вказано шлях
if (importPath is not null)
{
    if (!File.Exists(importPath))
    {
        Console.WriteLine($"Файл не знайдено: {Path.GetFullPath(importPath)}");
        return 1;
    }
    foreach (string line in ImportDispatcher.Load(importPath))
        Console.WriteLine(line);

    DomainDemo.ImportToDomain(importPath);
    DomainDemo.Run();
}

return 0;