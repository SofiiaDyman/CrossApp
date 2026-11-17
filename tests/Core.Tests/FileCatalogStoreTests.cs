using Core.Configuration;
using Core.Domain;
using Core.Storage;

namespace Core.Tests;

public class FileCatalogStoreTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
    private string FilePath => Path.Combine(_dir, "items.json");

    public void Dispose() { if (Directory.Exists(_dir)) Directory.Delete(_dir, true); }

    [Fact]
    public void Add_WritesReadableCyrillic_CamelCase_AndLeavesNoTmp()
    {
        new FileCatalogStore(FilePath).Add(Product.Create("P-1", "sku-1", "Магніт Львів", "магніт", "шт", 5));

        var json = File.ReadAllText(FilePath);
        Assert.Contains("Магніт Львів", json);
        Assert.DoesNotContain("\\u04", json);
        Assert.Contains("\"sku\"", json);
        Assert.False(File.Exists(FilePath + ".tmp"));
    }

    [Fact]
    public void NewInstance_ReadsWhatWasSaved()
    {
        new FileCatalogStore(FilePath).Add(Product.Create("P-1", "sku-1", "Чашка", "посуд", "шт", 7));

        var p = new FileCatalogStore(FilePath).GetById("P-1");
        Assert.NotNull(p);
        Assert.Equal("SKU-1", p!.Sku);
        Assert.Equal(7, p.Quantity);
    }

    [Fact]
    public void BrokenJson_ThrowsStorageException()
    {
        Directory.CreateDirectory(_dir);
        File.WriteAllText(FilePath, "[{ not json");
        Assert.Throws<StorageException>(() => new FileCatalogStore(FilePath).List());
    }

    [Fact]
    public void ResolveFullPath_RelativeUsesBaseDir_RootedIsKept()
    {
        var rel = new StorageOptions { DataPath = "data", FileName = "items.json" };
        Assert.Equal(Path.GetFullPath(Path.Combine(_dir, "data", "items.json")), rel.ResolveFullPath(_dir));
        Assert.True(Directory.Exists(Path.Combine(_dir, "data")));

        var abs = new StorageOptions { DataPath = Path.Combine(_dir, "abs"), FileName = "x.json" };
        Assert.Equal(Path.GetFullPath(Path.Combine(_dir, "abs", "x.json")), abs.ResolveFullPath("ignored"));
    }
}
