using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using CsvHelper;
using Flurl.Http;
using GeoWiki.Cli.Commands.PlanetApi.Models;
using IIASA.PlanetApi.Domain.Core;
using IIASA.PlanetApi.Domain.Models;
using Spectre.Console.Cli;

namespace GeoWiki.Cli.Commands.PlanetApi;

public class PlanetImageDownload : AsyncCommand<PlanetImageDownloadSettings>
{
    private Settings? _settings;

    public override async Task<int> ExecuteAsync(CommandContext context,
        PlanetImageDownloadSettings planetImageDownloadSettings)
    {
        ReadSettings(planetImageDownloadSettings.SettingsPath);

        var searchResults = SamplePointDataOuts(planetImageDownloadSettings);

        var sampleIdGroups =
            searchResults.GroupBy(x => x.SampleId).Select(s => new { SampleId = s.Key, data = s.ToList() }).ToArray();

        Console.WriteLine($"Found sampleId groups-{sampleIdGroups.Length}");
        var options = new JsonSerializerOptions
            { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull, WriteIndented = false };

        var index = 1;
        foreach (var group in sampleIdGroups)
        {
            var sampleId = group.SampleId;
            Console.WriteLine($"Processing sampleId-{sampleId} ...");
            var pointList = group.data;
            var samplePointDataOut = pointList.First();

            var geo = ConfigurationGenerator.AoiPolygon(samplePointDataOut.MinLat, samplePointDataOut.MinLong,
                samplePointDataOut.MaxLat, samplePointDataOut.MaxLong);

            var payload = PayLoad(sampleId, pointList, geo);

            var orderResult = OrderAndGetResult(payload, options);
            if (orderResult == null)
            {
                Console.WriteLine($"Error Processing sampleId-{sampleId} ...");
                continue;
            }

            Console.WriteLine($"Order placed for sampleId-{sampleId} ...");
            if (index++ % 4 == 0)
            {
                Thread.Sleep(TimeSpan.FromSeconds(1));
            }

            if (orderResult.StatusCode == (int)HttpStatusCode.Accepted)
            {
                await ProcessOrderResponse(orderResult, pointList, sampleId);
            }
            else
            {
                Console.WriteLine($"Failed to place Order for sampleId-{sampleId} ...");
            }
        }

        await WriteOutputCsv(planetImageDownloadSettings, searchResults);

        return 0;
    }

    private static async Task WriteOutputCsv(PlanetImageDownloadSettings planetImageDownloadSettings, List<SamplePointDataOut> searchResults)
    {
        var newFilePath = planetImageDownloadSettings.CsvPath.Replace(".csv", "_Ordered.csv");
        if (File.Exists(newFilePath))
        {
            newFilePath = planetImageDownloadSettings.CsvPath.Replace(".csv", $"{Guid.NewGuid():N}_Ordered.csv");
        }

        using (var writer = new StreamWriter(newFilePath))
        {
            using (var csvWriter = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csvWriter.Context.RegisterClassMap<SamplePointOrderMap>();
                await csvWriter.WriteRecordsAsync(searchResults);
            }
        }
    }

    private PayLoad PayLoad(string sampleId, List<SamplePointDataOut> pointList, Configuration geo)
    {
        var payload = ConfigurationGenerator.PayLoad($"Download {sampleId}",
            pointList.Select(x => x.SelectedImageId).Where(x => string.IsNullOrEmpty(x) == false).ToArray(),
            geo.Config);

        if (_settings == null)
        {
            throw new Exception("planetConfig.json not initialized");
        }

        payload.DeliveryObjects.AzureBlobStorage.Account = _settings.AzureConfig.Account;
        payload.DeliveryObjects.AzureBlobStorage.Container = _settings.AzureConfig.Account;
        payload.DeliveryObjects.AzureBlobStorage.SasToken = _settings.AzureConfig.SasToken;
        payload.DeliveryObjects.AzureBlobStorage.PathPrefix = sampleId;
        return payload;
    }

    private async Task ProcessOrderResponse(IFlurlResponse orderResult, List<SamplePointDataOut> samplePointList,
        string sampleId)
    {
        string responseString = await orderResult.GetStringAsync();
        var response = JsonSerializer.Deserialize<OrderResponse>(responseString);
        if (response == null)
        {
            return;
        }

        foreach (var pointDataOut in samplePointList)
        {
            pointDataOut.OrderId = response.Id;
            if (string.IsNullOrEmpty(pointDataOut.SelectedImageId) == false)
            {
                pointDataOut.TifUrl =
                    $"https://{_settings.AzureConfig.Account}.blob.core.windows.net/{_settings.AzureConfig.Container}/{sampleId}/{response.Id}/PSScene/{pointDataOut.SelectedImageId}_3B_Visual_clip_reproject.tif";
            }

            pointDataOut.OrderDetails = responseString;
        }
    }

    private static List<SamplePointDataOut> SamplePointDataOuts(PlanetImageDownloadSettings settings)
    {
        List<SamplePointDataOut> searchResults;
        using (var reader = new StreamReader(settings.CsvPath))
        {
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                csv.Context.RegisterClassMap<SamplePointDataOutMap>();
                searchResults = csv.GetRecords<SamplePointDataOut>().ToList();
            }
        }

        return searchResults;
    }


    IFlurlResponse? OrderAndGetResult(PayLoad payLoad, JsonSerializerOptions jsonSerializerOptions)
    {
        for (int tryIndex = 1; tryIndex < 5; tryIndex++)
        {
            try
            {
                var flurlResponse = _settings.OrderUrl.WithHeader("authorization", _settings.ApiKey)
                    .WithHeader("Content-Type", "application/json")
                    .PostAsync(JsonContent.Create(payLoad, new MediaTypeHeaderValue("application/json"),
                        jsonSerializerOptions))
                    .Result;
                return flurlResponse;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Thread.Sleep(TimeSpan.FromSeconds(10 * tryIndex));
            }
        }

        return null;
    }

    void ReadSettings(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            throw new Exception("planetConfig.json path not found!");
        }

        var text = File.ReadAllText(path);
        _settings = JsonSerializer.Deserialize<Settings>(text,
                        new JsonSerializerOptions
                            { MaxDepth = 2, PropertyNamingPolicy = JsonNamingPolicy.CamelCase }) ??
                    throw new InvalidOperationException();
    }
}