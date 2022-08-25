# geowiki-cli

Cli for the geowiki

## Add Shape file to postgis

### Environment variables required


## Planet API Image Download- https://www.planet.com/
Use this tool to search planetAPI images using sample point lat,long and cloud cover threshold. The using the search results download the required images with clipped AOI.
### Search-  geowiki planet-image-search -s <config.json> -p <csv path>  
This option allows you to provide planetAPI configuration which is ajson file with urls and other parameters which are self explainatory. Also it allows you to provide a csv file with lat. lang which will be used in search. The csv also has cloud cover which will be used to filter the images available at the location. Once the search results are output, user can have a look at csv and filter images by looking at the details and based on requirments. The Planet search API requires a valid API key but doesnot consume quota.

### Download- geowiki planet-image-download -s <config.json> -p <csv path>  
This option allows you to provide planetAPI configuration like same as before and a csv input which is the search results from above step. All images downloaded are placed in azure bloc store which is input in the config.json

###Config.json

```json
{
  "apiKey": "api-key your api key",
  "searchUrl": "https://api.planet.com/data/v1/quick-search",
  "orderUrl": "https://api.planet.com/compute/ops/orders/v2/",
  "startDate": "2022-01-01T00:00:00",
  "endDate": "2022-10-01T00:00:00",
  "azureConfig": {
    "account": "rapidai4eo",
    "container": "planet",
    "sasToken":  "Put your token",
    "pathPrefix": "output"
  }
}
```

###csv
sampleId | long_centroid | lat_centroid | long_min | long_max | lat_min | lat_max | cloudcover

