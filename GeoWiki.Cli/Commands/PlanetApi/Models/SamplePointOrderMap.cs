using CsvHelper.Configuration;

namespace GeoWiki.Cli.Commands.PlanetApi.Models;

public class SamplePointOrderMap : ClassMap<SamplePointDataOut>
{
    public SamplePointOrderMap()
    {
        Map(m => m.SampleId).Index(0).Name("id");
        Map(m => m.Long).Index(1).Name("Long");
        Map(m => m.Lat).Index(2).Name("Lat");
        Map(m => m.MinLong).Index(3).Name("MinLong");
        Map(m => m.MaxLong).Index(4).Name("MaxLong");
        Map(m => m.MinLat).Index(5).Name("MinLat");
        Map(m => m.MaxLat).Index(6).Name("MaxLat");
      
        Map(m => m.CloudCover).Index(7).Name("CloudCover");
        Map(m => m.StartDate).Index(8).Name("StartDate");
        Map(m => m.EndDate).Index(9).Name("EndDate");

        Map(m => m.SelectedImageId).Index(10).Name("SelectedImageId");
        Map(m => m.SelectedImageAcquiredDate).Index(11).Name("SelectedImageAcquiredDate");
        Map(m => m.SelectedImageCloudCover).Index(12).Name("SelectedImageCloudCover");
        Map(m => m.SelectedImageDetails).Index(13).Name("SelectedImageDetails");

        Map(m => m.OrderId).Index(14).Name("OrderId");
        Map(m => m.TifUrl).Index(15).Name("TifUrl");
        Map(m => m.OrderDetails).Index(16).Name("OrderDetails");
    }
}