using System;
using System.ComponentModel;
using GeoWiki.Cli.Services;
using Spectre.Console;
using Spectre.Console.Cli;
namespace GeoWiki.Cli.Commands.Resource;

public class ResourceCommand : Command<ResourceCommand.Settings>
{
    private readonly ResourceService _resourceService;

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

    public ResourceCommand(ResourceService resourceService)
    {
        _resourceService = resourceService ?? throw new ArgumentNullException(nameof(resourceService));
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        _resourceService.Import(settings.Path);
        return 0;
    }
}