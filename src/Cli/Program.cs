using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Encodings.Web;

Console.OutputEncoding = System.Text.Encoding.UTF8;

var info = new
{
    Student = "Софія Диман, група ФЕІ-36",
    OsDescription = RuntimeInformation.OSDescription,
    OsEnvironment = Environment.OSVersion.ToString(),
    ProcessArchitecture = RuntimeInformation.ProcessArchitecture.ToString(),
    ClrVersion = Environment.Version.ToString(),
    Runtime = RuntimeInformation.FrameworkDescription,
    AppBaseDirectory = AppContext.BaseDirectory,
    CurrentDirectory = Environment.CurrentDirectory,
    Domain = "Склад (товари, партії, залишки, переміщення)"
};

if (args.Contains("--json"))
{
    // Один рядок JSON, без відступів
    var options = new JsonSerializerOptions
    {
         Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
   };
    string json = JsonSerializer.Serialize(info, options);
    Console.WriteLine(json);
}
else
{
    // Вивід таблицею
    Console.WriteLine("CrossApp – практикум з крос-платформного програмування");
    Console.WriteLine($"Студент: {info.Student}");
    Console.WriteLine(new string('-', 60));
    Console.WriteLine($"{"Параметр",-25} | Значення");
    Console.WriteLine(new string('-', 60));
    Console.WriteLine($"{"ОС (OSDescription)",-25} | {info.OsDescription}");
    Console.WriteLine($"{"ОС (Environment)",-25} | {info.OsEnvironment}");
    Console.WriteLine($"{"Архітектура процесу",-25} | {info.ProcessArchitecture}");
    Console.WriteLine($"{"Версія .NET (CLR)",-25} | {info.ClrVersion}");
    Console.WriteLine($"{"Runtime",-25} | {info.Runtime}");
    Console.WriteLine($"{"Каталог застосунку",-25} | {info.AppBaseDirectory}");
    Console.WriteLine($"{"Поточний каталог",-25} | {info.CurrentDirectory}");
    Console.WriteLine(new string('-', 60));
    Console.WriteLine($"Предметна область: {info.Domain}");
}