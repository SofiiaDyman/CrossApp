using Core.Abstractions;
using Core.Domain;

namespace Core.Storage;

public sealed class CachingCatalogStore(ICatalogStore inner) : ICatalogStore
{
    private readonly ICatalogStore _inner = inner ?? throw new ArgumentNullException(nameof(inner));
    private readonly Dictionary<string, Product?> _byId = new(StringComparer.OrdinalIgnoreCase);
    private IReadOnlyList<Product>? _list;

    public int InnerCalls { get; private set; }   // щоб у звіті показати ефект кешу

    public IReadOnlyList<Product> List()
    {
        if (_list is null) { InnerCalls++; _list = _inner.List(); }
        return _list;
    }

    public Product? GetById(string id)
    {
        if (_byId.TryGetValue(id, out var cached)) return cached;
        InnerCalls++;
        var p = _inner.GetById(id);
        _byId[id] = p;
        return p;
    }

    public void Add(Product item)    { _inner.Add(item);    Invalidate(); }
    public void Update(Product item) { _inner.Update(item); Invalidate(); }
    public bool Remove(string id)    { var r = _inner.Remove(id); Invalidate(); return r; }

    private void Invalidate() { _list = null; _byId.Clear(); }
}