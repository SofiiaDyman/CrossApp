using Core.Services;

public static class StoreDemo
{
    public static void Run(CatalogService service)
    {
        Console.WriteLine();
        Console.WriteLine("=== Склад: сценарій сервісу ===");

        // 1. додати
        var created = service.Add("SKU-101", "Магніт Карпати", "магніт", "шт", 50);
        Console.WriteLine($"Додано: {created}");

        // 2. змінити (прихід і видача)
        service.Receive(created.Id, 25);
        service.Issue(created.Id, 10);

        // 3. знайти за id
        Console.WriteLine($"Знайдено: {service.Find(created.Id)}");

        // 4. список
        foreach (var p in service.All())
            Console.WriteLine($" {p.Id,-9} {p.Sku,-10} {p.Name,-28} {p.Quantity,5} {p.Unit}");

        // 5. пошук з предикатом (додаткове 2)
        var low = service.Search(p => p.Quantity < 50);
        Console.WriteLine($"Залишок < 50: {low.Count} поз.");

        // 6. відмови
        Try("прихід на неіснуючий id", () => service.Receive("no-such-id", 5));
        Try("видача більша за залишок", () => service.Issue(created.Id, 100000));
        Try("порожній SKU", () => service.Add(" ", "Х", "магніт", "шт", 1));
    }

    private static void Try(string title, Action action)
    {
        try
        {
            action();
            Console.WriteLine($"  {title}: помилки не було!");
        }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentException)
        {
            Console.WriteLine($"  Помилка ({title}): {ex.Message}");
        }
    }
}