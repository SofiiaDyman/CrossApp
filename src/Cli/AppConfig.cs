using System.Reflection;
using Core.Configuration;
using Microsoft.Extensions.Configuration;

public sealed record AppSettings(IConfiguration Config, AppOptions Options, string EnvName, string DataFile);

public static class AppConfig
{
    public static AppSettings Load(string[] args)
    {
        var envName = Environment.GetEnvironmentVariable("CROSSAPP_ENVIRONMENT") ?? "Development";

        var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .AddJsonFile($"appsettings.{envName}.json", optional: true)
            .AddUserSecrets(Assembly.GetExecutingAssembly(), optional: true)
            .AddEnvironmentVariables(prefix: "CROSSAPP_")
            .AddCommandLine(args)
            .Build();

        var options = config.Get<AppOptions>() ?? new AppOptions();

        var legacy = Environment.GetEnvironmentVariable("CROSSAPP_DATA_PATH");
        var storage = new StorageOptions
        {
            DataPath = string.IsNullOrWhiteSpace(legacy) ? options.Storage.DataPath : legacy,
            FileName = options.Storage.FileName
        };

        return new AppSettings(config, options, envName,
            storage.ResolveFullPath(AppContext.BaseDirectory));
    }

    public static void PrintSummary(AppSettings s)
    {
        Console.WriteLine($"Профіль    : {s.EnvName} ({s.Options.EnvironmentName})");
        Console.WriteLine($"Рівень логу: {s.Options.Logging.MinimumLevel}");
        Console.WriteLine($"Файл даних : {s.DataFile}");
        var apiKey = s.Config["Integration:ApiKey"];
        Console.WriteLine(string.IsNullOrEmpty(apiKey)
            ? "ApiKey: не заданий"
            : $"ApiKey: присутній, довжина {apiKey.Length}");
    }

    public static string? Arg(string[] args, string prefix) =>
        args.FirstOrDefault(a => a.StartsWith(prefix, StringComparison.Ordinal))?[prefix.Length..];
}
