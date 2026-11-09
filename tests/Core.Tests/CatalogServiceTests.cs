using Core.Events;
using Core.Services;
using Core.Storage;

namespace Core.Tests;

public class CatalogServiceTests
{
    [Fact] // 1) інваріант + незмінний стан
    public void Receive_WithNegativeQuantity_Throws_AndKeepsState()
    {
        var store = new InMemoryCatalogStore();
        var service = new CatalogService(store);
        var p = service.Add("SKU-1", "Кабель", "магніт", "шт", 10);

        Assert.Throws<ArgumentOutOfRangeException>(() => service.Receive(p.Id, -5));
        Assert.Equal(10, store.GetById(p.Id)!.Quantity);
    }

    [Fact] // 2) подія рівно один раз
    public void Add_RaisesChangedExactlyOnce()
    {
        var service = new CatalogService(new InMemoryCatalogStore());
        var events = new List<CatalogChangedEventArgs>();
        service.Changed += (_, e) => events.Add(e);

        service.Add("SKU-2", "Гвинт", "магніт", "шт", 7);

        Assert.Single(events);
        Assert.Equal(CatalogChangeKind.Added, events[0].Kind);
    }

    [Fact] // бізнес-правило -> DomainException
    public void Issue_MoreThanStock_ThrowsDomainException_AndKeepsState()
    {
        var store = new InMemoryCatalogStore();
        var service = new CatalogService(store);
        var p = service.Add("SKU-3", "Чашка", "посуд", "шт", 5);

        Assert.Throws<DomainException>(() => service.Issue(p.Id, 100));
        Assert.Equal(5, store.GetById(p.Id)!.Quantity);
    }
}