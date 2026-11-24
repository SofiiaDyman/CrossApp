using System.Security.Cryptography;
using Api.Contracts;
using Api.Validation;
using Core;
using Core.Abstractions;
using Core.Domain;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Api.Endpoints;

public static class ProductEndpoints
{
    public static void MapProducts(this WebApplication app)
    {
        var group = app.MapGroup("/api/products").WithTags("Products");

        group.MapGet("/", Results<Ok<PagedResult<ProductDto>>, ValidationProblem>
            (ICatalogStore store, int page = 1, int pageSize = 20, string? search = null) =>
        {
            var errors = new Dictionary<string, string[]>();
            if (page is < 1 or > 100_000) errors["page"] = ["page має бути від 1 до 100000."];
            if (pageSize is < 1 or > 100) errors["pageSize"] = ["pageSize має бути від 1 до 100."];
            if (errors.Count > 0) return TypedResults.ValidationProblem(errors);

            IEnumerable<Product> query = store.List();
            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                query = query.Where(p =>
                    p.Name.Contains(s, StringComparison.OrdinalIgnoreCase) ||
                    p.Sku.Contains(s, StringComparison.OrdinalIgnoreCase));
            }

            var filtered = query.OrderBy(p => p.Sku, StringComparer.Ordinal).ToList();
            var items = filtered.Skip((page - 1) * pageSize).Take(pageSize)
                                .Select(p => p.ToContract()).ToList();

            return TypedResults.Ok(new PagedResult<ProductDto>(items, filtered.Count));
        }).WithSummary("Список товарів з пагінацією та пошуком");

        group.MapGet("/low-stock", Results<Ok<List<ProductDto>>, ValidationProblem>
            (ICatalogStore store, int threshold = 5) =>
        {
            if (threshold <= 0)
                return TypedResults.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["threshold"] = ["threshold має бути більшим за 0."]
                });

            var low = store.List()
                .Where(p => p.Quantity < threshold)
                .OrderBy(p => p.Quantity).ThenBy(p => p.Sku, StringComparer.Ordinal)
                .Select(p => p.ToContract()).ToList();

            return TypedResults.Ok(low);
        }).WithSummary("Товари із залишком менше порогу");

        group.MapGet("/{id}", Results<Ok<ProductDto>, NotFound>
            (string id, ICatalogStore store) =>
            store.GetById(id) is { } found
                ? TypedResults.Ok(found.ToContract())
                : TypedResults.NotFound())
        .WithSummary("Товар за id");

        group.MapPost("/", (SaveProductRequest request, ICatalogStore store) =>
        {
            var id = Convert.ToHexString(RandomNumberGenerator.GetBytes(4)).ToLowerInvariant();
            var created = Product.Create(id, request.Sku, request.Name, request.Category,
                                         request.Unit, request.Quantity, request.Note);

            EnsureSkuUnique(store, created.Sku, exceptId: null);
            store.Add(created);

            return TypedResults.Created($"/api/products/{created.Id}", created.ToContract());
        })
        .AddEndpointFilter<ValidationFilter<SaveProductRequest>>()
        .ProducesValidationProblem()
        .ProducesProblem(StatusCodes.Status409Conflict)
        .WithSummary("Створити товар");

        group.MapPut("/{id}", Results<Ok<ProductDto>, NotFound>
            (string id, SaveProductRequest request, ICatalogStore store) =>
        {
            if (store.GetById(id) is null) return TypedResults.NotFound();

            var replacement = Product.Create(id, request.Sku, request.Name, request.Category,
                                             request.Unit, request.Quantity, request.Note);

            EnsureSkuUnique(store, replacement.Sku, exceptId: id);
            store.Update(replacement);

            return TypedResults.Ok(replacement.ToContract());
        })
        .AddEndpointFilter<ValidationFilter<SaveProductRequest>>()
        .ProducesValidationProblem()
        .ProducesProblem(StatusCodes.Status409Conflict)
        .WithSummary("Замінити товар");

        group.MapDelete("/{id}", Results<NoContent, NotFound>
            (string id, ICatalogStore store) =>
            store.Remove(id) ? TypedResults.NoContent() : TypedResults.NotFound())
        .WithSummary("Видалити товар");
    }

    private static void EnsureSkuUnique(ICatalogStore store, string sku, string? exceptId)
    {
        if (store.List().Any(p => p.Sku == sku &&
                                  !string.Equals(p.Id, exceptId, StringComparison.OrdinalIgnoreCase)))
            throw new DomainException($"Товар з артикулом {sku} уже існує.");
    }
}
