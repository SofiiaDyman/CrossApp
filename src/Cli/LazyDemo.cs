using Core.Domain;
using Core.Services;

public static class LazyDemo
{
    public static void Run(CatalogService service)
    {
        Console.WriteLine("=== Lab 07: відкладене виконання ===");

        var source = service.All().ToList();
        var query = source.Where(p => p.Quantity < 10);
        Console.WriteLine($"Дефіцитних до змін: {query.Count()}");

        source.Add(Product.Create("tmp00001", "SKU-999", "Скоба", "магніт", "шт", 3));
        Console.WriteLine($"Дефіцитних після змін: {query.Count()}");

        var snapshot = query.ToList();
        source.Clear();
        Console.WriteLine($"У знімку лишилось: {snapshot.Count}");
    }
}
