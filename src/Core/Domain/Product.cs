using Core;
using Core.Dto;

namespace Core.Domain;

public sealed class Product
{
    private static readonly string[] AllowedCategories =
    [
        "іграшка", "магніт", "дерево", "посуд",
        "стікер", "шкарпетки", "гра", "блокнот"
    ];

    private int _quantity;                    // приватне поле: залишок

    public string Id { get; }                 // get без set
    public string Sku { get; }
    public string Name { get; }
    public string Category { get; }
    public string Unit { get; }
    public string? Note { get; }
    public int Quantity => _quantity;         // читати можна, писати ззовні ні

    private Product(string id, string sku, string name, string category,
                    string unit, int quantity, string? note)
    {
        Id = id;
        Sku = sku;
        Name = name;
        Category = category;
        Unit = unit;
        _quantity = quantity;
        Note = note;
    }

    // Статичний метод, який перевіряє всі аргументи до створення об'єкта
    public static Product Create(string id, string sku, string name, string category,
                             string unit, int quantity, string? note = null)
{
    if (string.IsNullOrWhiteSpace(id))
        throw new ArgumentException("Ідентифікатор обов'язковий", nameof(id));
    if (string.IsNullOrWhiteSpace(sku))
        throw new ArgumentException("SKU не може бути порожнім", nameof(sku));
    if (string.IsNullOrWhiteSpace(name))
        throw new ArgumentException("Назва не може бути порожньою", nameof(name));
    if (string.IsNullOrWhiteSpace(unit))
        throw new ArgumentException("Одиниця виміру не може бути порожньою", nameof(unit));
    if (string.IsNullOrWhiteSpace(category))
        throw new ArgumentException("Категорія не може бути порожньою", nameof(category));

    string normalizedCategory = category.Trim().ToLowerInvariant();
    if (!AllowedCategories.Contains(normalizedCategory))
        throw new ArgumentException(
            $"Невідома категорія '{category}'. Допустимі: {string.Join(", ", AllowedCategories)}",
            nameof(category));

    if (quantity < 0)
        throw new ArgumentOutOfRangeException(nameof(quantity), quantity,
            "Початковий залишок не може бути від'ємним");

    return new Product(
        id.Trim(),
        sku.Trim().ToUpperInvariant(),
        name.Trim(),
        normalizedCategory,
        unit.Trim(),
        quantity,
        string.IsNullOrWhiteSpace(note) ? null : note.Trim());
}

public void RegisterArrival(int amount)
{
    if (amount <= 0)
        throw new ArgumentOutOfRangeException(nameof(amount), amount,
            "Кількість приходу має бути більшою за нуль");

    _quantity += amount;
}

public void Issue(int amount)
{
    if (amount <= 0)
        throw new ArgumentOutOfRangeException(nameof(amount), amount,
            "Кількість видачі має бути більшою за нуль");
    if (amount > _quantity)
        throw new DomainException(
            $"Не можна видати {amount}: залишок {Sku} = {_quantity}");

    _quantity -= amount;
}

public SouvenirDto ToDto() => new(Id, Sku, Name, Category, Unit, Quantity, Note);

public static Product FromDto(SouvenirDto dto)
{
    ArgumentNullException.ThrowIfNull(dto);
    return Create(dto.Id, dto.Sku, dto.Name, dto.Category, dto.Unit, dto.Quantity, dto.Note);
}

public override string ToString() => $"{Id} [{Sku}] {Name} ({Category}) — {Quantity} {Unit}";

}