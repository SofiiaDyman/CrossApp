using Core;
using Core.Events;
using Core.Services;

public static class EventDemo
{
    public static void Run(CatalogService service)
    {
        Console.WriteLine();
        Console.WriteLine("=== Lab 06: події ===");

        var logPath = Path.Combine(AppContext.BaseDirectory, "logs", "app.log");
        Directory.CreateDirectory(Path.GetDirectoryName(logPath)!);
        File.Delete(logPath);   // щоб у файлі був лише поточний запуск

        // Крок 3: локальний метод-підписник (консоль + файл)
        void OnCatalogChanged(object? sender, CatalogChangedEventArgs e)
        {
            var line = LogFormat.Line(e);
            Console.WriteLine(line);
            File.AppendAllText(logPath, line + Environment.NewLine);
        }
        service.Changed += OnCatalogChanged;

        // Крок 4: другий підписник — лямбда із замиканням над changes
        int changes = 0;
        service.Changed += (_, _) =>
        {
            changes++;
            Console.WriteLine($"    (лічильник: {changes})");   // видно порядок: спочатку лог, потім лічильник
        };

        // Крок 5
        var created = service.Add("SKU-101", "Магніт Карпати", "магніт", "шт", 50);
        service.Receive(created.Id, 25);
        Console.WriteLine($"Подій отримано: {changes}");

        // Крок 6: відмови — текст помилки є, рядка події немає
        TryFail("прихід з від'ємною кількістю", () => service.Receive(created.Id, -5));
        TryFail("прихід на неіснуючий id", () => service.Receive("no-such-id", 5));
        Console.WriteLine($"Після відмов подій: {changes} (не змінилось)");

        // Відписка: операція виконується, повідомлення в консолі/файлі немає
        service.Changed -= OnCatalogChanged;
        service.Receive(created.Id, 5);
        Console.WriteLine($"Подій отримано: {changes}");   // +1 лише від лямбди

        // Крок 7: Func замість події
        int threshold = 50;   // ще одне замикання
        var low = service.FindAll(p => p.Quantity < threshold);
        Console.WriteLine($"Залишок < {threshold}: {low.Count} поз.");
        foreach (var p in low) Console.WriteLine($"  {p}");
    }

    private static void TryFail(string title, Action action)
    {
        try { action(); Console.WriteLine($"  {title}: помилки не було!"); }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException or DomainException)
        { Console.WriteLine($"  Помилка ({title}): {ex.Message}"); }
    }
}