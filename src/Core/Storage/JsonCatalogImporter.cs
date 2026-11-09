using System.Runtime.CompilerServices;
using System.Text.Json;
using Core.Abstractions;
using Core.Domain;
using Core.Dto;

namespace Core.Storage;

public sealed class JsonCatalogImporter : ICatalogImporter
{
    private static readonly JsonSerializerOptions Options = new() { PropertyNameCaseInsensitive = true };

    public async Task<IReadOnlyList<Product>> LoadAsync(string path, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        List<SouvenirDto> dtos;
        try
        {
            await using var stream = Open(path);
            dtos = await JsonSerializer.DeserializeAsync<List<SouvenirDto>>(stream, Options, ct)
                ?? throw new DomainException($"Файл {Path.GetFileName(path)} не містить списку.");
        }
        catch (JsonException ex)
        {
            throw new StorageException($"Файл {Path.GetFileName(path)} не є коректним JSON", ex);
        }

        var result = new List<Product>(dtos.Count);
        for (var i = 0; i < dtos.Count; i++)
        {
            ct.ThrowIfCancellationRequested();
            result.Add(ToProduct(dtos[i], i + 1));
        }

        return result;
    }

    public async IAsyncEnumerable<Product> StreamAsync(
        string path, [EnumeratorCancellation] CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        await using var stream = Open(path);
        var number = 0;

        await foreach (var dto in JsonSerializer.DeserializeAsyncEnumerable<SouvenirDto>(stream, Options, ct))
        {
            ct.ThrowIfCancellationRequested();
            yield return ToProduct(dto, ++number);
        }
    }

    private static FileStream Open(string path) =>
        new(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true);

    private static Product ToProduct(SouvenirDto? dto, int number)
    {
        if (dto is { Quantity: 0 })
            throw new DomainException($"Запис {number}: кількість не може бути 0");

        try
        {
            return Product.FromDto(dto!);
        }
        catch (ArgumentException ex)
        {
            throw new DomainException($"Запис {number}: {ex.Message}", ex);
        }
    }
}
