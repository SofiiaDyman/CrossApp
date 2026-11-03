using Core.Domain;

namespace Core.Reports;

public static class ReportVariants
{
    private static IEnumerable<StockLine> BuildQuery(IEnumerable<Product> source, int threshold) =>
        source.Where(p => p.Quantity < threshold)
              .OrderBy(p => p.Quantity).ThenBy(p => p.Sku, StringComparer.Ordinal)
              .Select(p => new StockLine(p.Sku, p.Name, p.Quantity, p.Unit));

    public static int LazyUsedThreeTimes(IEnumerable<Product> source, int threshold)
    {
        var q = BuildQuery(source, threshold);
        return q.Count() + q.Sum(l => l.Quantity) + q.Count(l => l.Unit == "шт");
    }

    public static int MaterializedOnce(IEnumerable<Product> source, int threshold)
    {
        var l = BuildQuery(source, threshold).ToList();
        return l.Count + l.Sum(x => x.Quantity) + l.Count(x => x.Unit == "шт");
    }

    public static int TopNDirect(IEnumerable<Product> source, int n) =>
        source.OrderByDescending(p => p.Quantity).ThenBy(p => p.Sku, StringComparer.Ordinal)
              .Take(n).Count();

    public static int TopNWithMiddleToList(IEnumerable<Product> source, int n) =>
        source.OrderByDescending(p => p.Quantity).ThenBy(p => p.Sku, StringComparer.Ordinal)
              .ToList()
              .Take(n).Count();
}
