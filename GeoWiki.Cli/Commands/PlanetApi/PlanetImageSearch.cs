using System.Globalization;
using System.Text.Json;
using CsvHelper;
using GeoWiki.Cli.Commands.PlanetApi.Models;
using IIASA.PlanetApi.Domain.Models;
using Spectre.Console.Cli;

namespace GeoWiki.Cli.Commands.PlanetApi;

public class PlanetImageSearch : AsyncCommand<PlanetImageSearchSettings>
{
    private readonly IPlanetApiHelper _planetApiHelper;
    private Settings _settings = null!;

    public PlanetImageSearch(IPlanetApiHelper planetApiHelper)
    {
        _planetApiHelper = planetApiHelper;
    }
    public override async Task<int> ExecuteAsync(CommandContext context, PlanetImageSearchSettings planetImageSearchSettings)
    {
        _settings = ReadSettings(planetImageSearchSettings.SettingsPath);
        _planetApiHelper.Intialize(_settings);
        CheckParameters(planetImageSearchSettings);
        
        var samples = SamplePointDatas(planetImageSearchSettings);

        var samplePointDataOuts = _planetApiHelper.Search(samples.ToList());

        await WriteOutput(planetImageSearchSettings, samplePointDataOuts);

        return 0;
    }

    private static async Task WriteOutput(PlanetImageSearchSettings planetImageSearchSettings, List<SamplePointDataOut> samplePointDataOuts)
    {
        var newFile = planetImageSearchSettings.CsvPath.Replace(".csv", "_searchResults.csv");
        if (File.Exists(newFile))
        {
            newFile = planetImageSearchSettings.CsvPath.Replace(".csv", $"_{Guid.NewGuid():N}_searchResults.csv");
        }

        using (var writer = new StreamWriter(newFile))
        using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
        {
            csv.Context.RegisterClassMap<SamplePointDataOutMap>();
            await csv.WriteRecordsAsync(samplePointDataOuts);
        }
    }

    private static IEnumerable<SamplePointData> SamplePointDatas(PlanetImageSearchSettings settings)
    {
        IEnumerable<SamplePointData> samples;
        using (var reader = new StreamReader(settings.CsvPath))
        {
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                samples = csv.GetRecords<SamplePointData>().ToList();
            }
        }

        return samples;
    }

    private static async Task WriteOutput(string newFile, List<SamplePointDataOut> samplePointDataOuts)
    {
        using (var writer = new StreamWriter(newFile))
        using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
        {
            csv.Context.RegisterClassMap<SamplePointDataOutMap>();
            await csv.WriteRecordsAsync(samplePointDataOuts);
        }
    }

    private void CheckParameters(PlanetImageSearchSettings settings)
    {
        if (string.IsNullOrEmpty(settings.CsvPath) || File.Exists(settings.CsvPath) == false)
        {
            throw new Exception(
                "CSv Path is missing. Create a csv with headers - sampleId	long_centroid	lat_centroid	long_min	long_max	lat_min	lat_max	cloudcover");
        }

        if (string.IsNullOrEmpty(settings.SettingsPath) || File.Exists(settings.SettingsPath) == false)
        {
            throw new Exception(
                $"PlanetAPI Confi File is missing. Create a file with json- {JsonSerializer.Serialize(new Settings(), new JsonSerializerOptions() { WriteIndented = true })}");
        }
    }

    Settings ReadSettings(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            throw new Exception("planetConfig.json path not found!");
        }

        var text = File.ReadAllText(path);
        var settings = JsonSerializer.Deserialize<Settings>(text,
                        new JsonSerializerOptions
                            { MaxDepth = 2, PropertyNamingPolicy = JsonNamingPolicy.CamelCase }) ??
                    throw new InvalidOperationException();
        
        return settings;
    }
}