using System.Text.RegularExpressions;
using ClosedXML.Excel;
using GeoWiki.Cli.Proxy;
using Spectre.Console;
namespace GeoWiki.Cli.Services;

public class ImportService
{
    private readonly GeoWikiClient _geoWikiClient;
    private ICollection<KeyValuePairOfStringAndString> _resourceTopics;
    public ImportService(GeoWikiClient geoWikiClient)
    {
        _geoWikiClient = geoWikiClient ?? throw new ArgumentNullException(nameof(geoWikiClient));
    }
    
    public async Task ImportResource(string path)
    {
        AnsiConsole.MarkupLine($"Importing resources from [green]{path}[/]!");
        if (!File.Exists(path))
        {
            AnsiConsole.MarkupLine($"[red]Path does not exist[/]");
            return;
        }
        AnsiConsole.MarkupLine($"[green]Path exists[/]");
        AnsiConsole.MarkupLine($"[green]Importing[/]");
        using var workbook = new XLWorkbook(path);
        var resources = workbook.Worksheets.First();
        var rows = resources.RowsUsed();
        var columns = resources.ColumnsUsed();
        try
        {
            AnsiConsole.MarkupLine($"[green]Getting resource topics[/]");
            var resourceTopics = await _geoWikiClient.ResourceGetTopicAsync();
            _resourceTopics = resourceTopics;
        }
        catch (Exception e)
        {
            AnsiConsole.MarkupLine($"[red]Error while fetching resource topics[/]");
            AnsiConsole.WriteException(e);
        }
        
        _resourceTopics = await _geoWikiClient.ResourceGetTopicAsync();
        foreach (var row in rows.Skip(2).Take(2))
        {
            var title = row.Cell(3).GetString();
            if (!string.IsNullOrWhiteSpace(title))
            {
                AnsiConsole.MarkupLine($"[green]Importing row {title}[/]");
                var description = row.Cell(4).GetString();
                var author = row.Cell(5).GetString();
                var publisher = row.Cell(6).GetString();
                var year = row.Cell(7).GetString();
                var keywords = row.Cell(8).GetString();
                var resourceType = row.Cell(9).GetString();
                var topic = row.Cell(10).GetString();
                var audience = row.Cell(11).GetString();
                var language = row.Cell(12).GetString();
                var url = row.Cell(13).GetString();
                AnsiConsole.MarkupLine($"[green] Description {description} [/]");
                AnsiConsole.MarkupLine($"[green] Author {author} [/]");
                AnsiConsole.MarkupLine($"[green] Publisher {publisher} [/]");
                AnsiConsole.MarkupLine($"[green] Year {year} [/]");
                AnsiConsole.MarkupLine($"[green] Keywords {keywords} [/]");
                AnsiConsole.MarkupLine($"[green] ResourceType {CleanUpString(resourceType)} [/]");
                AnsiConsole.MarkupLine($"[green] Topic {GetTopic(topic)} [/]");
                AnsiConsole.MarkupLine($"[green] Audience {audience} [/]");
                AnsiConsole.MarkupLine($"[green] Language {language} [/]");
                AnsiConsole.MarkupLine($"[green] Url {url} [/]");
            }
        }
        AnsiConsole.MarkupLine($"[green]Found {rows.Count()} rows and {columns.Count()} columns[/]");
        AnsiConsole.MarkupLine($"[green]Done[/]");
    }

    private static string CleanUpString(string value)
    {
        // Remove values with in ()
        var newValue = Regex.Replace(value, @"\([^)]*\)", string.Empty);

        // Remove values with in []
        newValue = Regex.Replace(newValue, @"\[[^)]*\]", string.Empty);

        // Remove - and spaces
        newValue = newValue.Replace("-", string.Empty).Replace(" ", string.Empty);

        return newValue;
    }

    private string GetTopic(string value)
    {
        foreach (var item in _resourceTopics)
        {
            AnsiConsole.MarkupLine($"[green] {item.Value} [/]");
        }
        return value;
    }
}