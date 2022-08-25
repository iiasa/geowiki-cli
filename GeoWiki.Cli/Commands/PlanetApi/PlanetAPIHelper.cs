using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Flurl.Http;
using GeoWiki.Cli.Commands.PlanetApi.Models;
using IIASA.PlanetApi.Domain.Core;
using IIASA.PlanetApi.Domain.Models;

namespace GeoWiki.Cli.Commands.PlanetApi;

public interface IPlanetApiHelper
{
    List<SamplePointDataOut> Search(List<SamplePointData> samplePoints);
    void Intialize(Settings settings);
}

public class PlanetApiHelper : IPlanetApiHelper
{
    private Settings _settings;

    public void Intialize(Settings settings)
    {
        _settings = settings;
    }

    public List<SamplePointDataOut> Search(List<SamplePointData> samplePoints)
    {
        var sampleOutputs = samplePoints.Select(SamplePointDataOut.Initialize).ToList();
        var list = new List<SamplePointDataOut>();
        Console.WriteLine("Preparing the time range for all sample points...");
        foreach (var pointDataOut in sampleOutputs)
        {
            var date = _settings.StartDate;
            while (date <= _settings.EndDate)
            {
                var newData = pointDataOut.Clone();
                newData.StartDate = date;
                newData.EndDate = date.AddMonths(1).Subtract(TimeSpan.FromDays(1));
                list.Add(newData);
                date = date.AddMonths(1);
            }
        }

        var total = list.Count;
        Console.WriteLine($"Completed. Total points generated -{total}");

        var concurrentTasks = 5;
        var batch = (total / concurrentTasks) + 5;
        var tasks = new List<Task>();
        for (int i = 0; i < total; i += batch)
        {
            var batchList = list.Skip(i).Take(batch).ToList();
            var index = i;

            var task = Task.Factory.StartNew( () =>
            {
                BatchProcess(batchList, _settings.SearchUrl, _settings.ApiKey, index).Wait();
            });
            tasks.Add(task);
        }
        
        Task.WaitAll(tasks.ToArray());
        return list;
    }

    private async Task BatchProcess(List<SamplePointDataOut> list, string searchUrl, string apiKey, int taskNumber)
    {
        var total = list.Count;
        var errors = 1;
        for (var index = 0; index < list.Count; index++)
        {
            var samplePoint = list[index];
            try
            {
                var options = new JsonSerializerOptions
                    { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull, WriteIndented = false };
                Console.WriteLine($"{taskNumber} Searching for sample point images. remaining-{total--}");
                var response = await SearchImages(samplePoint, searchUrl, apiKey, taskNumber);
                var images = response.Features;
                images = images.OrderBy(x => x.Properties.CloudCover).ToList();
                if (images.Count > 0)
                {
                    var selected = images.First();
                    if (selected.Properties.CloudCover < samplePoint.CloudCover)
                    {
                        samplePoint.SelectedImageId = selected.Id;
                        samplePoint.SelectedImageCloudCover = selected.Properties.CloudCover;
                        samplePoint.SelectedImageAcquiredDate = selected.Properties.Acquired;
                        samplePoint.SelectedImageDetails = $"'{JsonSerializer.Serialize(selected, options)}'";
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"{taskNumber} error-" + e);
                Thread.Sleep(TimeSpan.FromSeconds(10 * errors++));
                index--;
            }
        }
    }

    private static async Task<Response> SearchImages(SamplePointDataOut samplePoint, string searchUrl, string apiKey,
        int taskNumber)
    {
        var msg =
            $"{taskNumber} {samplePoint.SampleId} StartDate-{samplePoint.StartDate:d} endDate-{samplePoint.EndDate:d}...";
        Console.WriteLine($"{taskNumber} Searching for images -{msg}");
        var geoFilter = ConfigurationGenerator.AoiPolygon(samplePoint.MinLat, samplePoint.MinLong,
            samplePoint.MaxLat, samplePoint.MaxLong);

        var dateFilter =
            ConfigurationGenerator.DateRange(samplePoint.StartDate, samplePoint.EndDate);

        var cloudCoverFilter = ConfigurationGenerator.CloudCover(0.1);

        var combinedFilter = ConfigurationGenerator.CombineFilters(new List<Configuration>
            { geoFilter, dateFilter, cloudCoverFilter });

        var request = ConfigurationGenerator.SearchRequest(combinedFilter);

        var options = new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };

        var result = await searchUrl.WithHeader("authorization", apiKey)
            .PostAsync(JsonContent.Create(request, new MediaTypeHeaderValue("application/json"), options));

        if (result.StatusCode != (int)HttpStatusCode.OK)
        {
            return new Response { Features = new List<Feature>() };
        }

        var response = JsonSerializer.Deserialize<Response>(await result.GetStringAsync());
        Console.WriteLine($"{taskNumber} Completed-{msg}");
        return response;
    }
}