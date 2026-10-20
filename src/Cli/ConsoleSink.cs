using Core.Abstractions;
using Core.Events;

public sealed class ConsoleSink : INotificationSink
{
    public void Notify(CatalogChangedEventArgs e) =>
        Console.WriteLine($"[sink:console] {LogFormat.Line(e)}");
}