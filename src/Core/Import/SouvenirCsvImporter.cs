using System.Globalization;
using Core.Dto;

namespace Core.Import;

public static class SouvenirCsvImporter
{
    private const char Separator = ';';

    private static readonly string[] AllowedCategories =
    {
        "іграшка", "магніт", "дерево", "посуд",
        "стікер", "шкарпетки", "гра", "блокнот"
    };

    public static ImportResult<SouvenirDto> Load(string path)
    {
        var items = new List<SouvenirDto>();
        var errors = new List<string>();

        string[] lines = File.ReadAllLines(path, System.Text.Encoding.UTF8);

        for (int i = 0; i < lines.Length; i++)
        {
            int number = i + 1;
            string line = lines[i];

            if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
                continue;

            if (number == 1 && line.StartsWith("id", StringComparison.OrdinalIgnoreCase))
                continue; // рядок заголовків

            switch (ParseLine(line))
            {
                case ParseOk ok:
                    items.Add(ok.Value);
                    break;
                case ParseFailed failed:
                    errors.Add($"рядок {number}: {failed.Reason}");
                    break;
            }
        }

        return new ImportResult<SouvenirDto>(items, errors);
    }

    private static ParseOutcome ParseLine(string line)
    {
        string[] parts = line.Split(Separator, StringSplitOptions.TrimEntries);

        return parts switch
        {
            { Length: < 6 } =>
                new ParseFailed($"очікую щонайменше 6 колонок, отримав {parts.Length}"),

            [_, "", _, _, _, _, ..] or [_, _, "", _, _, _, ..] =>
                new ParseFailed("SKU або назва порожні"),

            [_, _, _, var category, _, _, ..] when !AllowedCategories.Contains(category) =>
                new ParseFailed($"невідома категорія '{category}'"),

            [_, _, _, _, _, var qty, ..] when !int.TryParse(qty, NumberStyles.Integer, CultureInfo.InvariantCulture, out int q) || q < 0 =>
                new ParseFailed($"кількість '{qty}' не є невід'ємним числом"),

            [var id, var sku, var name, var category, var unit, var qty] =>
                new ParseOk(new SouvenirDto(id, sku, name, category, unit, int.Parse(qty, CultureInfo.InvariantCulture))),

            [var id, var sku, var name, var category, var unit, var qty, var note] =>
                new ParseOk(new SouvenirDto(id, sku, name, category, unit, int.Parse(qty, CultureInfo.InvariantCulture), note)),

            _ => new ParseFailed($"занадто багато колонок: {parts.Length}")
        };
    }

    private abstract record ParseOutcome;
    private sealed record ParseOk(SouvenirDto Value) : ParseOutcome;
    private sealed record ParseFailed(string Reason) : ParseOutcome;
}