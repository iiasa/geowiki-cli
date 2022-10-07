# geowiki command line tool

Cli for the geowiki

## Add Shape file to PostGis

```bash
geowiki add shapefile --path /path/to/file.shp --table table_name
```

> **Important**: The shapefile must be in the same projection as the database. These environment variables are required for this command to work: `POSTGRES_HOST`, `POSTGRES_PORT`, `POSTGRES_USER`, `POSTGRES_PASSWORD`, `POSTGRES_DATABASE`

## Planet API Image Download- <https://www.planet.com/>

Use this tool to search planetAPI images using sample point lat,long and cloud cover threshold. Then using the search results download the required images with clipped AOI.

### Search Planet API

```bash
geowiki planet image-search -s <config.json> -p <csv path> 
```

This option allows you to provide planetAPI configuration which is a json file with urls and other parameters which are self explanatory. Also it allows you to provide a csv file with lat. lang which will be used in search query to planet. The csv has cloud cover which will be used to filter the images available at the location. Once the search results are output, user can have a look at csv and filter images by looking at the details and based on requirements. The Planet search API requires a valid API key but does not consume quota.

### Download Planet API Images

```bash
geowiki planet image-download -s <config.json> -p <csv path> 
```

This option allows you to provide planet API configuration like same as before and a csv input which is the search results from above step. All images downloaded are placed in azure blob store which is input in the config.json. The Planet API Image download tool requires a valid API key and quota to download the images.

The downloaded image is automatically clipped to the AOI mentioned in the csv file using clip tool. Also the downloaded image is re-projected to more usable EPSG4326 using re-project tool from planet API- <https://developers.planet.com/apis/orders/tools-toolchains/>

### Config.json

```json
{
  "apiKey": "api-key <your api key>",
  "searchUrl": "https://api.planet.com/data/v1/quick-search",
  "orderUrl": "https://api.planet.com/compute/ops/orders/v2/",
  "startDate": "2022-01-01T00:00:00",
  "endDate": "2022-10-01T00:00:00",
  "azureConfig": {
    "account": "rapidai4eo",
    "container": "planet",
    "sasToken":  "<Put your token>",
    "pathPrefix": "output"
  }
}
```

### CSV File Format

```csv
sampleId | long_centroid | lat_centroid | long_min | long_max | lat_min | lat_max | cloudcover
```
