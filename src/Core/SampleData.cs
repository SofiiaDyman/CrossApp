using Core.Domain;

namespace Core;

public static class SampleData
{
    public static IReadOnlyList<Product> Products() =>
    [
        Product.Create("P-001", "SKU-001", "М'яка іграшка Ведмедик", "іграшка", "шт", 45),
        Product.Create("P-002", "SKU-002", "Магніт Львів", "магніт", "шт", 120),
        Product.Create("P-003", "SKU-003", "Дерев'яний магніт-планшет", "дерево", "шт", 60),
        Product.Create("P-004", "SKU-004", "Чашка з принтом міста", "посуд", "шт", 80),
        Product.Create("P-005", "SKU-005", "Бокал скляний сувенірний", "посуд", "шт", 35),
        Product.Create("P-006", "SKU-006", "Стікерпак Україна", "стікер", "уп", 200),
        Product.Create("P-007", "SKU-007", "Шкарпетки з вишивкою", "шкарпетки", "пара", 160),
        Product.Create("P-008", "SKU-008", "Настільна гра Мафія", "гра", "шт", 25),
        Product.Create("P-009", "SKU-009", "Блокнот шкіряний А5", "блокнот", "шт", 70),
        Product.Create("P-010", "SKU-010", "М'яка іграшка Заєць", "іграшка", "шт", 55, "Хіт продажів"),
        Product.Create("P-011", "SKU-011", "Магніт дерев'яний Карпати", "дерево", "шт", 90),
        Product.Create("P-012", "SKU-012", "Магніт Ратуша", "магніт", "шт", 110),
        Product.Create("P-013", "SKU-013", "Чашка керамічна", "посуд", "шт", 40),
        Product.Create("P-014", "SKU-014", "Стікерпак Львів", "стікер", "уп", 160),
        Product.Create("P-015", "SKU-015", "Шкарпетки Карпати", "шкарпетки", "пара", 95),
        Product.Create("P-016", "SKU-016", "Блокнот А6 крафт", "блокнот", "шт", 30),
        Product.Create("P-017", "SKU-017", "Дерев'яний брелок Лев", "дерево", "шт", 8),
        Product.Create("P-018", "SKU-018", "Набір стікерів Кав'ярні", "стікер", "комп", 5),
        Product.Create("P-019", "SKU-019", "Пазл Ратуша", "гра", "комп", 70),
        Product.Create("P-020", "SKU-020", "Шкарпетки дитячі", "шкарпетки", "пара", 9),
    ];

    public static IReadOnlyList<StockMovement> Movements() =>
    [
        new("P-001", 20, new DateOnly(2025, 3, 1)),
        new("P-001", -5, new DateOnly(2025, 3, 4)),
        new("P-002", -30, new DateOnly(2025, 3, 2)),
        new("P-004", 50, new DateOnly(2025, 3, 3)),
        new("P-006", -15, new DateOnly(2025, 3, 5)),
        new("P-017", -2, new DateOnly(2025, 3, 6)),
        new("P-999", 10, new DateOnly(2025, 3, 7)),
    ];

    public static IReadOnlyList<Product> Generate(int count, int seed = 42)
    {
        var rnd = new Random(seed);
        string[] categories = ["іграшка", "магніт", "дерево", "посуд", "стікер", "шкарпетки", "гра", "блокнот"];
        string[] units = ["шт", "уп", "пара", "комп"];

        return Enumerable.Range(1, count)
            .Select(i => Product.Create($"G-{i:D6}", $"GEN-{i:D6}", $"Товар {i}",
                categories[rnd.Next(categories.Length)], units[rnd.Next(units.Length)], rnd.Next(0, 1000)))
            .ToList();
    }
}