namespace Api.Contracts;

public sealed record SaveProductRequest(
    string Sku, string Name, string Category, string Unit, int Quantity, string? Note = null);
