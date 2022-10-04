using System;
using System.ComponentModel;
using Spectre.Console.Cli;
using GeoWiki.Cli.Services;
using Spectre.Console;

namespace GeoWiki.Cli.Commands.Import;

public class ResourceImportCommand : Command<ResourceImportCommand.Settings>
{
    private readonly ImportService _importService;

    public sealed class Settings : CommandSettings
    {
        [CommandOption("-p|--path <Path>")]
        [Description("Import resources.")]
        public string? Path { get; init; }

        public override ValidationResult Validate()
        {
            return string.IsNullOrWhiteSpace(Path) ? ValidationResult.Error("Path is required.") : ValidationResult.Success();
        }
    }

    public ResourceImportCommand(ImportService importService)
    {
        _importService = importService ?? throw new ArgumentNullException(nameof(importService));
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        _importService.ImportResource(settings.Path);
        return 0;
    }
}