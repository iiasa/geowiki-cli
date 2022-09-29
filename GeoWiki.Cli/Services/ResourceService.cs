using Spectre.Console;
namespace GeoWiki.Cli.Services;

public class ResourceService
{
    public void Greet(string name)
    {
        AnsiConsole.MarkupLine($"Hello, [green]{name}[/]!");
    }
}