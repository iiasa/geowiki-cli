using System.ComponentModel;
using Spectre.Console.Cli;

namespace GeoWiki.Cli.Commands.AddShapeFile;

[Description("Adds a shape file to the database")]
public class AddShapeFileSettings : CommandSettings
{
    [CommandOption("-p|--path <PATH>")]
    [Description("The path to the shape file")]
    public string FilePath { get; set; } = "";

    [CommandOption("-t|--table <TABLE>")]
    [Description("The name of the table to create")]
    public string? TableName { get; set; }
}