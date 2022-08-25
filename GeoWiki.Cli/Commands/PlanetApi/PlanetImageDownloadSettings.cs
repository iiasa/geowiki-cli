using System.ComponentModel;
using Spectre.Console.Cli;

namespace GeoWiki.Cli.Commands.PlanetApi;

[Description("Download Planet API for Images from search results")]
public class PlanetImageDownloadSettings : CommandSettings
{
    [CommandOption("-s|--config <PATH>")]
    [Description("The path to the planetConfig.json")]
    public string SettingsPath { get; set; } = ".";

    [CommandOption("-p|--path <PATH>")]
    [Description("The path to the csv file containing search results")]
    public string CsvPath { get; set; } = ".";
}