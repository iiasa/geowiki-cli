using Spectre.Console;
namespace GeoWiki.Cli.Services;

public class ResourceService
{
    public void Greet(string name)
    {
        AnsiConsole.MarkupLine($"Hello, [green]{name}[/]!");
    }

    public void Import(string path)
    {
        AnsiConsole.MarkupLine($"Importing resources from [green]{path}[/]!");
        if(!System.IO.Directory.Exists(path))
        {
            AnsiConsole.MarkupLine($"[red]Path does not exist[/]");
            return;
        }

        // read excel file from the path
        // import resources
        var file = File.ReadAllBytes(path);
        var excelPackage = new ExcelPackage(new MemoryStream(file));

    }
}