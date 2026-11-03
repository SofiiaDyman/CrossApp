using Core.Domain;

namespace Core.Reports;

public static class ReportService
{
    private static StockLine ToLine(Product p) => new(p.Sku, p.Name, p.Quantity, p.Unit);

    public static IReadOnlyList<StockLine> LowStock(IEnumerable<Product> source, int threshold) =>
        source.Where(p => p.Quantity < threshold)
              .OrderBy(p => p.Quantity).ThenBy(p => p.Sku, StringComparer.Ordinal)
              .Select(ToLine)
              .ToList();

    public static IReadOnlyList<StockLine> TopByQuantity(IEnumerable<Product> source, int n) =>
        source.TopBy(p => p.Quantity, n, p => p.Sku)
              .Select(ToLine)
              .ToList();

    public static IReadOnlyList<GroupSummary> ByUnit(IEnumerable<Product> source) =>
        source.GroupBy(p => p.Unit, StringComparer.OrdinalIgnoreCase)
              .Select(g => new GroupSummary(g.Key, g.Count(), g.Sum(p => p.Quantity)))
              .OrderByDescending(s => s.TotalQuantity).ThenBy(s => s.Key, StringComparer.Ordinal)
              .ToList();

    public static IReadOnlyList<GroupSummary> TopUnits(IEnumerable<Product> source, int n) =>
        ByUnit(source).TopBy(s => s.TotalQuantity, n, s => s.Key).ToList();

    public static IReadOnlyList<GroupSummary> ByUnitQuerySyntax(IEnumerable<Product> source) =>
        (from p in source
         group p by p.Unit.ToLowerInvariant() into g
         orderby g.Sum(p => p.Quantity) descending, g.Key
         select new GroupSummary(g.Key, g.Count(), g.Sum(p => p.Quantity))).ToList();

    public static (int Items, int Total, double Average, int Max) Totals(IEnumerable<Product> source)
    {
        var list = source.ToList();
        return list.Count == 0
            ? (0, 0, 0d, 0)
            : (list.Count, list.Sum(p => p.Quantity), list.Average(p => p.Quantity), list.Max(p => p.Quantity));
    }

    public static IReadOnlyList<MovementLine> Movements(
        IEnumerable<Product> products, IEnumerable<StockMovement> movements) =>
        products.Join(movements,
                      p => p.Id,
                      m => m.ProductId,
                      (p, m) => new MovementLine(p.Sku, p.Name, m.Delta, m.Date),
                      StringComparer.OrdinalIgnoreCase)
                .OrderBy(l => l.Date).ThenBy(l => l.Sku, StringComparer.Ordinal)
                .ToList();

    public static IReadOnlyList<MovementSummary> MovementSummaries(
        IEnumerable<Product> products, IEnumerable<StockMovement> movements) =>
        products.GroupJoin(movements,
                           p => p.Id,
                           m => m.ProductId,
                           (p, ms) => new MovementSummary(p.Sku, p.Name, ms.Count(), ms.Sum(m => m.Delta)),
                           StringComparer.OrdinalIgnoreCase)
                .OrderBy(s => s.NetDelta).ThenBy(s => s.Sku, StringComparer.Ordinal)
                .ToList();
}
