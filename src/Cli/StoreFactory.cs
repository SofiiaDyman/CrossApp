using Core;
using Core.Abstractions;
using Core.Storage;

public static class StoreFactory
{
    public static ICatalogStore Create(string[] args)
    {
        string dataPath = Path.Combine(AppContext.BaseDirectory, "data", "catalog.json");

        ICatalogStore store = args.Contains("--file")
            ? new FileCatalogStore(dataPath)
            : new InMemoryCatalogStore(SampleData.Products());

        return args.Contains("--cache") ? new CachingCatalogStore(store) : store;
    }
}