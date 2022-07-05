$apiKey = $args[0]

Write-Host $apiKey

# Delete old packages
del .\GeoWiki.Cli\nupkg\*.nupkg

dotnet pack .\GeoWiki.Cli\GeoWiki.Cli.csproj -c release
