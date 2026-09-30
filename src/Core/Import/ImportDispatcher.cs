using Core.Dto;

namespace Core.Import;

public static class ImportDispatcher
{
    public static IReadOnlyList<string> Load(string path)
    {
        if (Path.GetFileName(path).Equals("mixed.csv", StringComparison.OrdinalIgnoreCase))
            return FormatMixed(MixedCsvImporter.Load(path));

        ImportResult<SouvenirDto> result = path.EndsWith(".json", StringComparison.OrdinalIgnoreCase)
            ? SouvenirJsonImporter.Load(path)
            : SouvenirCsvImporter.Load(path);

        return FormatSouvenirs(result);
    }

    private static IReadOnlyList<string> FormatMixed(MixedImportResult mixed)
    {
        var lines = new List<string>
        {
            $"Товарів: {mixed.Souvenirs.Count}, Постачальників: {mixed.Suppliers.Count}"
        };

        foreach (SouvenirDto s in mixed.Souvenirs)
            lines.Add($" [P] {s.Id,-6} {s.Name,-30} {s.Category,-10} {s.Quantity,5} {s.Unit}");

        foreach (SupplierDto s in mixed.Suppliers)
            lines.Add($" [S] {s.Id,-8} {s.Name,-25} {s.Phone}");

        if (mixed.Errors.Count > 0)
        {
            lines.Add($"Пропущено рядків: {mixed.Errors.Count}");
            foreach (string e in mixed.Errors)
                lines.Add($" ! {e}");
        }

        return lines;
    }

    private static IReadOnlyList<string> FormatSouvenirs(ImportResult<SouvenirDto> result)
    {
        var lines = new List<string> { $"Завантажено записів: {result.Items.Count}" };

        foreach (SouvenirDto p in result.Items.Take(5))
            lines.Add($" {p.Id,-6} {p.Sku,-10} {p.Name,-26} {p.Quantity,5} {p.Unit}");

        if (result.Errors.Count > 0)
        {
            lines.Add($"Пропущено рядків: {result.Errors.Count}");
            foreach (string e in result.Errors)
                lines.Add($" ! {e}");
        }

        int total = result.Items.Count + result.Errors.Count;
        double errorRate = total == 0 ? 0 : result.Errors.Count * 100.0 / total;
        lines.Add($"Статистика: усього {total}, прийнято {result.Items.Count}, " +
                   $"пропущено {result.Errors.Count}, % помилок {errorRate:F1}%");

        return lines;
    }
}