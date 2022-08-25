﻿using GeoWiki.Cli;
using GeoWiki.Cli.Commands.AddShapeFile;
using GeoWiki.Cli.Commands.Default;
using GeoWiki.Cli.Commands.PlanetApi;
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
        registrations.AddScoped<IPlanetApiHelper, PlanetApiHelper>();
        var registrar = new TypeRegistrar(registrations);

        var app = new CommandApp(registrar);
        app.Configure(config =>
        {
            config.SetApplicationName("geowiki");
            config.AddCommand<DefaultCommand>("hello");
            config.AddCommand<AddShapeFileCommand>("add-shape-file");
            config.AddCommand<PlanetImageSearch>("planet-image-search");
            config.AddCommand<PlanetImageDownload>("planet-image-download");
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