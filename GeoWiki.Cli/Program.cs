using GeoWiki.Cli;
using GeoWiki.Cli.Commands.Add;
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
        // Create a type registrar and register any dependencies.
        // A type registrar is an adapter for a DI framework.
        var registrations = new ServiceCollection();
        registrations.AddSingleton<IGreeter, HelloWorldGreeter>();
        registrations.AddScoped<ShapeFileService>();
        var registrar = new TypeRegistrar(registrations);

        // Create a new command app with the registrar
        // and run it with the provided arguments.
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