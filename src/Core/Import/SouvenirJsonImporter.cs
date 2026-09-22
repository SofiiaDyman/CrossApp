using System.Text.Json;
using Core.Dto;

namespace Core.Import;

public static class SouvenirJsonImporter
{
    public static ImportResult<SouvenirDto> Load(string path)
    {
        var errors = new List<string>();
        List<SouvenirDto> items;

        try
        {
            string json = File.ReadAllText(path, System.Text.Encoding.UTF8);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            items = JsonSerializer.Deserialize<List<SouvenirDto>>(json, options) ?? [];
        }
        catch (JsonException ex)
        {
            errors.Add($"помилка розбору JSON: {ex.Message}");
            items = [];
        }

        return new ImportResult<SouvenirDto>(items, errors);
    }
}