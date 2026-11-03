using System.Diagnostics;
using Core;
using Core.Reports;

public static class BenchDemo
{
    public static void Run()
    {
        Console.WriteLine("=== Бенчмарк (100 000 записів; краще запускати з -c Release) ===");
        var data = SampleData.Generate(100_000);
        const int threshold = 500;

        Measure("Lazy: запит перебирається 3 рази", () => ReportVariants.LazyUsedThreeTimes(data, threshold));
        Measure("ToList один раз, далі 3 використання", () => ReportVariants.MaterializedOnce(data, threshold));
        Measure("Топ-5 без ToList усередині", () => ReportVariants.TopNDirect(data, 5));
        Measure("Топ-5 з ToList після сортування", () => ReportVariants.TopNWithMiddleToList(data, 5));
    }

    private static void Measure(string title, Func<int> action, int runs = 20)
    {
        action();
        long sink = 0;
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < runs; i++) sink += action();
        sw.Stop();
        Console.WriteLine($" {title,-42} {sw.Elapsed.TotalMilliseconds / runs,8:F2} мс/запуск  (контрольна сума {sink})");
    }
}
