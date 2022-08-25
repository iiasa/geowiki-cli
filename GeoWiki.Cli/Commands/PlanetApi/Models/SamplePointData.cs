using CsvHelper.Configuration.Attributes;

namespace GeoWiki.Cli.Commands.PlanetApi.Models;

public class SamplePointData
{
    [Index(0)] public string SampleId { get; set; }
    [Index(1)] public double Long { get; set; }
    [Index(2)] public double Lat { get; set; }
    [Index(3)] public double MinLong { get; set; }
    [Index(4)] public double MaxLong { get; set; }
    [Index(5)] public double MinLat { get; set; }
    [Index(6)] public double MaxLat { get; set; }
    [Index(7)] public double CloudCover { get; set; } // from 0 to 1.
}