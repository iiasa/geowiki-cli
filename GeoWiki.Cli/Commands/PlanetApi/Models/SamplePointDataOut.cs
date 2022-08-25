namespace GeoWiki.Cli.Commands.PlanetApi.Models;

public class SamplePointDataOut : SamplePointData
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string SelectedImageId { get; set; }
    public string SelectedImageDetails { get; set; }
    public double SelectedImageCloudCover { get; set; }
    public DateTime? SelectedImageAcquiredDate { get; set; }

    public string OrderId { get; set; }

    public string TifUrl { get; set; }

    public string OrderDetails { get; set; }

    public SamplePointDataOut Clone()
    {
        return new SamplePointDataOut
        {
            SampleId = SampleId,
            Lat = Lat,
            Long = Long,
            MinLat = MinLat,
            MaxLat = MaxLat,
            MinLong = MinLong,
            MaxLong = MaxLong,
            CloudCover = CloudCover
        };
    }

    public static SamplePointDataOut Initialize(SamplePointData data)
    {
        return new SamplePointDataOut
        {
            SampleId = data.SampleId,
            Lat = data.Lat,
            Long = data.Long,
            MinLat = data.MinLat,
            MaxLat = data.MaxLat,
            MinLong = data.MinLong,
            MaxLong = data.MaxLong,
            CloudCover = data.CloudCover
        };
    }
}