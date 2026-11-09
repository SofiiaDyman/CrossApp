using Core.Reports;

namespace Core.Tests;

public class ReportServiceTests
{
    [Fact] // 3)
    public void LowStock_CountsOnlyBelowThreshold()
    {
        Assert.Equal(2, ReportService.LowStock(TestData.Items(), 10).Count);
        Assert.Empty(ReportService.LowStock(TestData.Items(), 1));
    }

    [Fact] // 4)
    public void TopByQuantity_IsDeterministic() =>
        Assert.Equal(new[] { "SKU-BIG", "SKU-MID" },
            ReportService.TopByQuantity(TestData.Items(), 2).Select(l => l.Sku));
}