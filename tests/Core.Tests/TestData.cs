using Core.Domain;

namespace Core.Tests;

internal static class TestData
{
    // 2 позиції < 10, жодної < 1; два найбільші: BIG, MID
    public static IReadOnlyList<Product> Items() =>
    [
        Product.Create("T-1", "SKU-BIG", "Великий", "магніт", "шт", 100),
        Product.Create("T-2", "SKU-MID", "Середній", "посуд", "шт", 50),
        Product.Create("T-3", "SKU-LOW", "Малий", "гра", "шт", 3),
        Product.Create("T-4", "SKU-MIN", "Мінімум", "блокнот", "шт", 7),
    ];
}