$apiKey = $args[0]

Write-Host $apiKey

# Delete old packages
del .\GeoWiki.Cli\nupkg\*.nupkg

dotnet pack .\GeoWiki.Cli\GeoWiki.Cli.csproj -c release
$file = Get-Item -Path .\GeoWiki.Cli\nupkg\*.nupkg | %{$_.FullName}

Write-Host $file

dotnet nuget push .\GeoWiki.Cli\nupkg\*.nupkg --skip-duplicate -s https://api.nuget.org/v3/index.json --api-key "$apiKey"