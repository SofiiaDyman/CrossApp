using Core.Abstractions;
using Core.Events;

public sealed class FileSink : INotificationSink
{
    private readonly string _path;

    public FileSink(string path)
    {
        _path = path;
        Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
    }

    public void Notify(CatalogChangedEventArgs e) =>
        File.AppendAllText(_path, LogFormat.Line(e) + Environment.NewLine);
}