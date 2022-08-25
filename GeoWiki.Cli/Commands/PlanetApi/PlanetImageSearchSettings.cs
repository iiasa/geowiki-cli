using System.ComponentModel;
using Spectre.Console.Cli;

namespace GeoWiki.Cli.Commands.PlanetApi;

[Description("Searches Planet API for Images")]
public class PlanetImageSearchSettings : CommandSettings
{
    [CommandOption("-s|--settingspath <PATH>")]
    [Description("The path to the planetConfig.json")]
    public string SettingsPath { get; set; } = ".";

    [CommandOption("-p|--csvpath <PATH>")]
    [Description("The path to the csv file containing lat, long")]
    public string CsvPath { get; set; } = ".";
}