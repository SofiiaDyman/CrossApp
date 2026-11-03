using Core.Abstractions;
using Core.Domain;
using Core.Events;

namespace Core.Services;

public sealed class CatalogService(ICatalogStore store, IEnumerable<INotificationSink>? sinks = null)
{
    private readonly ICatalogStore _store = store ?? throw new ArgumentNullException(nameof(store));
    private readonly List<INotificationSink> _sinks = sinks?.ToList() ?? [];

    public event EventHandler<CatalogChangedEventArgs>? Changed;

    // єдине місце підняття події
    private void OnChanged(Product p, CatalogChangeKind kind, int delta, int? quantityAfter = null)
    {
        var args = new CatalogChangedEventArgs(p.Id, kind, delta, quantityAfter ?? p.Quantity);
        Changed?.Invoke(this, args);
        foreach (var sink in _sinks) sink.Notify(args);   // додаткове 1
    }

    public Product Add(string sku, string name, string category, string unit, int quantity)
    {
        var product = Product.Create(Guid.NewGuid().ToString("N")[..8],
                                     sku, name, category, unit, quantity);
        _store.Add(product);
        OnChanged(product, CatalogChangeKind.Added, quantity);      // ПІСЛЯ зміни
        return product;
    }

    public void Receive(string id, int quantity)
    {
        var product = _store.GetById(id)
            ?? throw new InvalidOperationException($"Немає запису з id={id}.");
        product.RegisterArrival(quantity);       // виняток -> події не буде
        _store.Update(product);
        OnChanged(product, CatalogChangeKind.Received, quantity);
    }

    public void Issue(string id, int quantity)
    {
        var product = _store.GetById(id)
            ?? throw new InvalidOperationException($"Немає запису з id={id}.");
        product.Issue(quantity);
        _store.Update(product);
        OnChanged(product, CatalogChangeKind.Issued, -quantity);    // видача = мінус
    }

    public bool Remove(string id)
    {
        var product = _store.GetById(id);
        if (product is null) return false;
        int before = product.Quantity;
        if (!_store.Remove(id)) return false;
        OnChanged(product, CatalogChangeKind.Removed, -before, quantityAfter: 0);
        return true;
    }

    public IReadOnlyList<Product> All() => _store.List();
    public Product? Find(string id) => _store.GetById(id);

    // Func-пошук
    public IReadOnlyList<Product> FindAll(Func<Product, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        return _store.List().Where(predicate).ToList();
    }
}