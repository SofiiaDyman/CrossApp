using System.Globalization;
using Core.Dto;

namespace Core.Import;

public sealed record MixedImportResult(
    IReadOnlyList<SouvenirDto> Souvenirs,
    IReadOnlyList<SupplierDto> Suppliers,
    IReadOnlyList<string> Errors);

public static class MixedCsvImporter
{
    private const char Separator = ';';

    public static MixedImportResult Load(string path)
    {
        var souvenirs = new List<SouvenirDto>();
        var suppliers = new List<SupplierDto>();
        var errors = new List<string>();

        string[] lines = File.ReadAllLines(path, System.Text.Encoding.UTF8);

        for (int i = 0; i < lines.Length; i++)
        {
            int number = i + 1;
            string line = lines[i];
            if (string.IsNullOrWhiteSpace(line)) continue;

            string[] parts = line.Split(Separator, StringSplitOptions.TrimEntries);

            switch (parts)
            {
                case ["P", var id, var sku, var name, var category, var unit, var qty]
                    when int.TryParse(qty, NumberStyles.Integer, CultureInfo.InvariantCulture, out int q) && q >= 0:
                    souvenirs.Add(new SouvenirDto(id, sku, name, category, unit, q));
                    break;

                case ["S", var id, var name]:
                    suppliers.Add(new SupplierDto(id, name));
                    break;

                case ["S", var id, var name, var phone]:
                    suppliers.Add(new SupplierDto(id, name, phone));
                    break;

                case ["P", ..]:
                    errors.Add($"рядок {number}: некоректний рядок товару");
                    break;

                case ["S", ..]:
                    errors.Add($"рядок {number}: некоректний рядок постачальника");
                    break;

                default:
                    errors.Add($"рядок {number}: невідомий префікс типу '{parts.FirstOrDefault()}'");
                    break;
            }
        }

        return new MixedImportResult(souvenirs, suppliers, errors);
    }
}