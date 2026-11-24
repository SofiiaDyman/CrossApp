using Core.Domain;

namespace Api.Contracts;

public sealed record ProductDto(
    string Id, string Sku, string Name, string Category,
    string Unit, int Quantity, string? Note);

public static class ProductMapping
{
    public static ProductDto ToContract(this Product p) =>
        new(p.Id, p.Sku, p.Name, p.Category, p.Unit, p.Quantity, p.Note);
}
