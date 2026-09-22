using Core.Dto;
using Core.Import;

string path = args.Length > 0 ? args[0] : Path.Combine("data", "sample.csv");

if (!File.Exists(path))
{
    Console.WriteLine($"Файл не знайдено: {Path.GetFullPath(path)}");
    return 1;
}

if (Path.GetFileName(path).Equals("mixed.csv", StringComparison.OrdinalIgnoreCase))
{
    MixedImportResult mixed = MixedCsvImporter.Load(path);

    Console.WriteLine($"Товарів: {mixed.Souvenirs.Count}, Постачальників: {mixed.Suppliers.Count}");

    foreach (SouvenirDto s in mixed.Souvenirs)
        Console.WriteLine($" [P] {s.Id,-6} {s.Name,-30} {s.Category,-10} {s.Quantity,5} {s.Unit}");

    foreach (SupplierDto s in mixed.Suppliers)
        Console.WriteLine($" [S] {s.Id,-8} {s.Name,-25} {s.Phone}");

    if (mixed.Errors.Count > 0)
    {
        Console.WriteLine($"Пропущено рядків: {mixed.Errors.Count}");
        foreach (string e in mixed.Errors)
            Console.WriteLine($" ! {e}");
    }

    return 0;
}

ImportResult<SouvenirDto> result = path.EndsWith(".json", StringComparison.OrdinalIgnoreCase)
    ? SouvenirJsonImporter.Load(path)
    : SouvenirCsvImporter.Load(path);

Console.WriteLine($"Завантажено записів: {result.Items.Count}");

foreach (SouvenirDto p in result.Items.Take(5))
    Console.WriteLine($" {p.Id,-6} {p.Sku,-10} {p.Name,-30} {p.Category,-10} {p.Quantity,5} {p.Unit}");

if (result.Errors.Count > 0)
{
    Console.WriteLine($"Пропущено рядків: {result.Errors.Count}");
    foreach (string e in result.Errors)
        Console.WriteLine($" ! {e}");
}

int total = result.Items.Count + result.Errors.Count;
double errorRate = total == 0 ? 0 : result.Errors.Count * 100.0 / total;
Console.WriteLine($"Статистика: усього {total}, прийнято {result.Items.Count}, " +
                   $"пропущено {result.Errors.Count}, % помилок {errorRate:F1}%");

return 0;