using System.ComponentModel;
using Spectre.Console.Cli;

namespace GeoWiki.Cli.Commands.Add;
public class AddSettings : CommandSettings
{
    [CommandOption("-p|--path <PATH>")]
    [Description("Path of the file.")]
    public string FilePath { get; set; }
}