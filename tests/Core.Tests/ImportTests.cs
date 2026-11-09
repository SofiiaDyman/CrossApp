using Core.Events;
using Core.Services;
using Core.Storage;

namespace Core.Tests;

public class ImportTests
{
    private const string BadSku =
        """[{"Id":"P-1","Sku":"","Name":"X","Category":"магніт","Unit":"шт","Quantity":5}]""";
    private const string ZeroQty =
        """[{"Id":"P-1","Sku":"SKU-1","Name":"X","Category":"магніт","Unit":"шт","Quantity":0}]""";
    private const string TwoGood =
        """
        [{"Id":"P-1","Sku":"SKU-1","Name":"X","Category":"магніт","Unit":"шт","Quantity":5},
         {"Id":"P-2","Sku":"SKU-2","Name":"Y","Category":"посуд","Unit":"шт","Quantity":6}]
        """;

    [Fact] // 5) попередньо скасований токен
    public async Task ImportAsync_WhenAlreadyCancelled_Throws()
    {
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();
        var service = new CatalogService(new InMemoryCatalogStore());

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => service.ImportAsync(new JsonCatalogImporter(), "any.json", cts.Token));
    }

    [Fact] // 6) некоректний запис -> DomainException з номером
    public Task LoadAsync_BadRecord_ThrowsDomainException_WithRecordNumber() =>
        TempFile.WithAsync(BadSku, async path =>
        {
            var ex = await Assert.ThrowsAsync<DomainException>(
                () => new JsonCatalogImporter().LoadAsync(path));
            Assert.Contains("Запис 1", ex.Message);
            Assert.IsAssignableFrom<ArgumentException>(ex.InnerException);
        });

    [Fact] // 7) склад: кількість 0
    public Task LoadAsync_ZeroQuantity_ThrowsDomainException() =>
        TempFile.WithAsync(ZeroQty, async path =>
        {
            var ex = await Assert.ThrowsAsync<DomainException>(
                () => new JsonCatalogImporter().LoadAsync(path));
            Assert.Contains("кількість не може бути 0", ex.Message);
        });

    [Fact] // 8) зламаний JSON -> StorageException з inner
    public Task LoadAsync_BrokenJson_ThrowsStorageException_WithInner() =>
        TempFile.WithAsync("[{ not json", async path =>
        {
            var ex = await Assert.ThrowsAsync<StorageException>(
                () => new JsonCatalogImporter().LoadAsync(path));
            Assert.IsAssignableFrom<System.Text.Json.JsonException>(ex.InnerException);
        });

    [Fact] // 9) події для кожного запису + ідемпотентність
    public async Task ImportAsync_RaisesEventPerItem_AndIsIdempotent()
    {
        var service = new CatalogService(new InMemoryCatalogStore());
        var events = new List<CatalogChangedEventArgs>();
        service.Changed += (_, e) => events.Add(e);
        var importer = new ListImporter(TestData.Items());

        Assert.Equal(4, await service.ImportAsync(importer, "x"));
        Assert.Equal(4, events.Count);
        Assert.Equal(0, await service.ImportAsync(importer, "x"));
        Assert.Equal(4, events.Count);
    }

    [Fact] // 10) прогрес закінчується на 100 і не спадає
    public async Task ImportAsync_ReportsProgress_UpTo100()
    {
        var service = new CatalogService(new InMemoryCatalogStore());
        var progress = new ListProgress();

        await service.ImportAsync(new ListImporter(TestData.Items()), "x", progress: progress);

        Assert.Equal(100, progress.Values[^1]);
        Assert.Equal(progress.Values.OrderBy(v => v), progress.Values);
    }

    [Fact] // 11) зв'язані токени: спрацював саме таймаут, а не користувач
    public async Task ImportAsync_Timeout_CancelsThroughLinkedToken_NotUser()
    {
        using var user = new CancellationTokenSource();
        using var timeout = new CancellationTokenSource(TimeSpan.FromMilliseconds(50));
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(user.Token, timeout.Token);
        var service = new CatalogService(new InMemoryCatalogStore());

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => service.ImportAsync(new HangingImporter(), "x", linked.Token));

        Assert.True(timeout.IsCancellationRequested);
        Assert.False(user.IsCancellationRequested);
    }

    [Fact] // 12) стрімінговий імпорт
    public Task ImportStreamAsync_ImportsAllRecords() =>
        TempFile.WithAsync(TwoGood, async path =>
        {
            var service = new CatalogService(new InMemoryCatalogStore());
            Assert.Equal(2, await service.ImportStreamAsync(new JsonCatalogImporter(), path));
        });

    [Fact] // 13) стрімінг + скасований токен
    public Task ImportStreamAsync_WhenCancelled_Throws() =>
        TempFile.WithAsync(TwoGood, async path =>
        {
            using var cts = new CancellationTokenSource();
            await cts.CancelAsync();
            var service = new CatalogService(new InMemoryCatalogStore());
            await Assert.ThrowsAnyAsync<OperationCanceledException>(
                () => service.ImportStreamAsync(new JsonCatalogImporter(), path, cts.Token));
        });
}