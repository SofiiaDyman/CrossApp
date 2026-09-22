namespace Core.Dto;

public record SouvenirDto(
    string Id,
    string Sku,
    string Name,
    string Category,
    string Unit,
    int Quantity,
    string? Note = null);