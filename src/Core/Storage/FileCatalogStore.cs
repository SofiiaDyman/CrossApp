using System.Text.Json;
using Core.Abstractions;
using Core.Domain;
using Core.Dto;

namespace Core.Storage;

public sealed class FileCatalogStore(string path) : ICatalogStore
{
    private readonly Dictionary<string, Product> _cache = new(StringComparer.OrdinalIgnoreCase);
    private readonly string _path = Path.GetFullPath(path);
    private bool _loaded;

    private void EnsureLoaded()
    {
        if (_loaded) return;
        if (!File.Exists(_path))
        {
            _loaded = true;
            return;
        }

        List<SouvenirDto> dtos;
        try
        {
            dtos = JsonSerializer.Deserialize<List<SouvenirDto>>(File.ReadAllText(_path), CatalogJson.Options) ?? [];
        }
        catch (JsonException ex)
        {
            throw new StorageException($"Файл {_path} пошкоджено: {ex.Message}", ex);
        }

        _cache.Clear();
        for (var i = 0; i < dtos.Count; i++)
        {
            try
            {
                var product = Product.FromDto(dtos[i]);
                _cache[product.Id] = product;
            }
            catch (ArgumentException ex)
            {
                throw new DomainException($"{_path}, запис {i + 1}: {ex.Message}", ex);
            }
        }

        _loaded = true;
    }

    private void Flush()
    {
        var tmp = _path + ".tmp";
        Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
        File.WriteAllText(tmp, JsonSerializer.Serialize(_cache.Values.Select(p => p.ToDto()).ToList(), CatalogJson.Options));
        File.Move(tmp, _path, overwrite: true);
    }

    public int ReplaceAll(IEnumerable<Product> items)
    {
        ArgumentNullException.ThrowIfNull(items);
        _cache.Clear();
        foreach (var p in items) _cache[p.Id] = p;
        _loaded = true;
        Flush();
        return _cache.Count;
    }

    public IReadOnlyList<Product> List() { EnsureLoaded(); return _cache.Values.ToList(); }
    public Product? GetById(string id) { EnsureLoaded(); return _cache.GetValueOrDefault(id); }

    public void Add(Product item)
    {
        ArgumentNullException.ThrowIfNull(item);
        EnsureLoaded();
        if (_cache.ContainsKey(item.Id))
            throw new InvalidOperationException($"Запис з id={item.Id} уже існує.");
        _cache.Add(item.Id, item);
        Flush();
    }

    public void Update(Product item)
    {
        ArgumentNullException.ThrowIfNull(item);
        EnsureLoaded();
        _cache[item.Id] = item;
        Flush();
    }

    public bool Remove(string id)
    {
        EnsureLoaded();
        if (!_cache.Remove(id)) return false;
        Flush();
        return true;
    }
}