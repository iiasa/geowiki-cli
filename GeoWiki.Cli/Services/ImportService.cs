using System.Text.RegularExpressions;
using ClosedXML.Excel;
using GeoWiki.Cli.Proxy;
using Spectre.Console;
namespace GeoWiki.Cli.Services;

public class ImportService
{
    private readonly GeoWikiClient _geoWikiClient;
    private readonly AuthService _authService;
    private ICollection<KeyValuePairOfStringAndString> _resourceTopics;
    public ImportService(GeoWikiClient geoWikiClient, AuthService authService)
    {
        AnsiConsole.MarkupLine($"[green]ImportService[/]");
        _geoWikiClient = geoWikiClient ?? throw new ArgumentNullException(nameof(geoWikiClient));
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
    }

    public async Task ImportResource(string path)
    {
        if(!await _authService.IsAuthenticatedAsync())
        {
            AnsiConsole.MarkupLine($"[red]You are not authenticated. Please login first.[/]");
            AnsiConsole.MarkupLine($"[red]Use the command 'geowiki login' to login.[/]");
            return;
        }
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
        foreach (var row in rows.Skip(2))
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
                var resource = new CreateUpdateResourceDto
                {
                    Title = title,
                    Description = description,
                    Author = author,
                    Publisher = publisher,
                    Keywords = string.IsNullOrWhiteSpace(keywords) ? null : keywords.Split(',').Select(x => x.Trim()).ToList(),
                    ResourceType = CleanUpString(resourceType),
                    Topic = GetTopic(topic),
                    Audience = GetAudience(audience),
                    Link = url,
                    Language = string.IsNullOrWhiteSpace(language) ? null : language.Split(',').Select(x => x.Trim()).ToList(),
                };
                // AnsiConsole.MarkupLine($"[green] Description {description} [/]");
                // AnsiConsole.MarkupLine($"[green] Author {author} [/]");
                // AnsiConsole.MarkupLine($"[green] Publisher {publisher} [/]");
                // AnsiConsole.MarkupLine($"[green] Year {year} [/]");
                // AnsiConsole.MarkupLine($"[green] Keywords {keywords} [/]");
                // AnsiConsole.MarkupLine($"[green] ResourceType {CleanUpString(resourceType)} [/]");
                // AnsiConsole.MarkupLine($"[green] Audience {audience}[/]");
                // AnsiConsole.MarkupLine($"[green] Language {language} [/]");
                // AnsiConsole.MarkupLine($"[green] Url {url} [/]");
                var result = await _geoWikiClient.ResourceCreateAsync(resource);
                if (result == null)
                {
                    AnsiConsole.MarkupLine($"[red]Error while creating resource[/]");
                }
                AnsiConsole.MarkupLine($"[green]Resource created[/]");
            }
        }
        AnsiConsole.MarkupLine($"[green]Done[/]");
    }

    private List<string> GetAudience(string audience)
    {
        var audiences = audience.Split(',');
        var audienceIds = new List<string>();
        foreach (var a in audiences)
        {
            var audienceId = GetAudienceByCode(a);
            if (!string.IsNullOrWhiteSpace(audienceId))
            {
                audienceIds.Add(audienceId);
            }
        }
        return audienceIds;
    }

    private string GetAudienceByCode(string audienceCode)
    {
        switch (audienceCode.Trim())
        {
            case "F":
                return "Farmers";
            case "FA":
                return "FarmerClusterFacilitatorAndAdvisor";
            case "PM":
                return "PolicyDecisionMaker";
            case "SR":
                return "ScientistAndResearcher";
            default:
                return "GeneralPublic";
        }
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

    private List<string> GetTopic(string value)
    {
        var topics = value.Split(',');
        var topicResults = new List<string>();
        foreach (var topic in topics)
        {
            var topicName = topic.Trim();
            var topicResult = GetTopicNameByCode(topicName);
            topicResults.Add(topicResult);
        }
        return topicResults;
    }

    private string GetTopicNameByCode(string topicName)
    {
        var topics = _resourceTopics.Select(x =>
        {
            var firstLetters = x.Value.Split(' ').Select(x => x[0]).ToArray();
            var topic = new string(firstLetters);
            if (topic == "B")
            {
                topic = "BD";
            }
            if (topic == topicName)
            {
                return x.Key;
            }
            return string.Empty;
        });

        return string.Join("", topics);
    }
}