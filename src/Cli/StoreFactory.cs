using Core;
using Core.Abstractions;
using Core.Storage;

public static class StoreFactory
{
    public static ICatalogStore Create(string[] args, string dataFile)
    {
        ICatalogStore store = args.Contains("--file")
            ? new FileCatalogStore(dataFile)
            : new InMemoryCatalogStore(SampleData.Products());

        return args.Contains("--cache") ? new CachingCatalogStore(store) : store;
    }
}