using Core.Domain;
using Core.Dto;
using Core.Import;

public static class DomainDemo
{
    public static void ImportToDomain(string path)
    {
        if (Path.GetFileName(path).Equals("mixed.csv", StringComparison.OrdinalIgnoreCase))
            return;

        Console.WriteLine();
        Console.WriteLine("=== DTO -> сутності ===");
        ProductImportOutcome outcome = ProductImportMapper.ToDomain(ImportDispatcher.LoadSouvenirs(path));
        Console.WriteLine($"Створено сутностей: {outcome.Products.Count}, відхилено доменом: {outcome.Rejected.Count}");
        foreach (string r in outcome.Rejected)
            Console.WriteLine($" ! {r}");
    }

    public static void Run()
    {
        Console.WriteLine();
        Console.WriteLine("=== Сценарій 1: успіх ===");
        Product product = Product.Create("P-001", "sku-001", "Магніт Львів", "магніт", "шт", 100);
        Console.WriteLine(product);
        product.RegisterArrival(50);
        product.Issue(30);
        Console.WriteLine(product);

        //product.Quantity = 5;

        var request = IssueRequest.Create("R-001", product, "C-77", 20, new DateOnly(2025, 3, 1));
        request.Confirm();
        Console.WriteLine($"Заявка {request.Id}: {request.Status}, залишок {product.Quantity}");

        Product restored = Product.FromDto(product.ToDto());
        Console.WriteLine($"Відновлено з DTO: {restored}");

        Console.WriteLine();
        Console.WriteLine("=== Сценарій 2: порушення інваріантів ===");
        TryDo("видача більша за залишок", () => product.Issue(1000));
        TryDo("порожній SKU", () => Product.Create("P-002", " ", "Чашка", "посуд", "шт", 10));
        TryDo("від'ємний залишок", () => Product.Create("P-003", "SKU-003", "Чашка", "посуд", "шт", -5));
        TryDo("невідома категорія", () => Product.Create("P-004", "SKU-004", "Меч", "гра настільна", "шт", 1));
        TryDo("нульова кількість приходу", () => product.RegisterArrival(0));
        TryDo("повторне підтвердження заявки", () => request.Confirm());
        TryDo("скасування підтвердженої заявки", () => request.Cancel());
        TryDo("FromDto з пошкодженими даними",
              () => Product.FromDto(new SouvenirDto("P-005", "", "Х", "магніт", "шт", 5)));
        TryDo("6-та відкрита заявка одержувача", () =>
        {
            var list = Enumerable.Range(1, 5)
                .Select(i => IssueRequest.Create($"R-1{i}", product, "C-99", 1, new DateOnly(2025, 3, 1)))
                .ToList();
            IssuePolicy.EnsureRecipientCanOpen(list, "C-99");
        });
        Console.WriteLine($"Після всіх відмов: {product}");
    }

    private static void TryDo(string title, Action action)
    {
        try
        {
            action();
            Console.WriteLine($"  {title}: виняток НЕ спрацював — інваріант відсутній!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  {title}: {ex.GetType().Name} — {ex.Message}");
        }
    }
}