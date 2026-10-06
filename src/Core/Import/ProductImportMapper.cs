using Core.Domain;
using Core.Dto;

namespace Core.Import;

public sealed record ProductImportOutcome(
    IReadOnlyList<Product> Products,
    IReadOnlyList<string> Rejected);

public static class ProductImportMapper
{
    public static ProductImportOutcome ToDomain(ImportResult<SouvenirDto> result)
    {
        var products = new List<Product>();
        var rejected = new List<string>();

        foreach (SouvenirDto dto in result.Items)
        {
            try
            {
                products.Add(Product.FromDto(dto));
            }
            catch (ArgumentException ex)   // ловить і ArgumentOutOfRangeException
            {
                rejected.Add($"{dto.Id}: {ex.Message}");
            }
        }

        return new ProductImportOutcome(products, rejected);
    }
}