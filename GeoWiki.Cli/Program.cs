using GeoWiki.Cli.Commands.AddShapeFile;
using GeoWiki.Cli.Commands.Login;
using GeoWiki.Cli.Commands.PlanetApi;
// PLOP_INJECT_USING
using GeoWiki.Cli.Commands.ResourceDeleteAll;
using GeoWiki.Cli.Commands.SwitchTenant;
using GeoWiki.Cli.Commands.Import;
using GeoWiki.Cli.Handlers;
using GeoWiki.Cli.Infrastructure;
using GeoWiki.Cli.Proxy;
using GeoWiki.Cli.Services;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using Spectre.Console.Cli;

public static class Program
{
    public static int Main(string[] args)
    {
        // args = new[] { "resource", "import",  "-p", @"C:\Users\antos\Downloads\infohub2.xlsx" };

        var registrations = new ServiceCollection();
        registrations.AddScoped<ShapeFileService>();
        registrations.AddScoped<AuthService>();
        registrations.AddScoped<DatabaseService>();
        registrations.AddScoped<IPlanetApiHelper, PlanetApiHelper>();
        registrations.AddScoped<HeaderHandler>();

        // PLOP_SERVICE_REGISTRATION
        registrations.AddScoped<ResourceService>();
        registrations.AddScoped<SwitchService>();

        registrations.AddHttpClient<GeoWikiClient>(client =>
        {
            client.BaseAddress = new Uri(Constants.ApiUrl);

        })
        .AddHttpMessageHandler<HeaderHandler>();

        var registrar = new TypeRegistrar(registrations);

        var app = new CommandApp(registrar);
        app.Configure(config =>
        {
            config.SetApplicationName("geowiki");
            config.AddCommand<LoginCommand>("login");

            // PLOP_COMMAND_REGISTRATION

            config.AddBranch("resource", import =>
            {
                import.AddCommand<ResourceImportCommand>("import");
                import.AddCommand<ResourceDeleteAllCommand>("delete-all");
            });

            config.AddBranch("tenant", change =>
            {
                change.AddCommand<SwitchTenantCommand>("switch");
            });

            config.AddBranch("add", add =>
            {
                add.AddCommand<AddShapeFileCommand>("shapefile");
            });

            config.AddBranch("planet", planet =>
            {
                planet.AddCommand<PlanetImageSearch>("image-search");
                planet.AddCommand<PlanetImageDownload>("image-download");
            });
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