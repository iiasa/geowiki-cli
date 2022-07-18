using GeoWiki.Cli;
using GeoWiki.Cli.Commands.AddShapeFile;
using GeoWiki.Cli.Commands.Default;
using GeoWiki.Cli.Infrastructure;
using GeoWiki.Cli.Services;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using Spectre.Console.Cli;

public static class Program
{
    public static int Main(string[] args)
    {
        var registrations = new ServiceCollection();
        registrations.AddSingleton<IGreeter, HelloWorldGreeter>();
        registrations.AddScoped<ShapeFileService>();
        registrations.AddScoped<DatabaseService>();
        var registrar = new TypeRegistrar(registrations);

        var app = new CommandApp(registrar);
        app.Configure(config =>
        {
            config.SetApplicationName("geowiki");
            config.AddCommand<DefaultCommand>("hello");
            config.AddCommand<AddShapeFileCommand>("add-shape-file");
        });
        try
        {
            return app.Run(args);
        }
        catch (Exception e)
        {
            AnsiConsole.WriteException(e);
        }
        return 1;
    }
}