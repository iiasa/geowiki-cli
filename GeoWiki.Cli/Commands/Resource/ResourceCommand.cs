using System;
using System.ComponentModel;
using GeoWiki.Cli.Services;
using Spectre.Console.Cli;
namespace GeoWiki.Cli.Commands.Resource;

public class ResourceCommand : Command<ResourceCommand.Settings>
{
    private readonly ResourceService _resourceService;

    public sealed class Settings : CommandSettings
    {
        [CommandOption("-n|--name <NAME>")]
        [Description("The person or thing to greet.")]
        public string Name { get; set; } = "World";
    }

    public ResourceCommand(ResourceService resourceService)
    {
        _resourceService = resourceService ?? throw new ArgumentNullException(nameof(resourceService));
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        _resourceService.Greet(settings.Name);
        return 0;
    }
}