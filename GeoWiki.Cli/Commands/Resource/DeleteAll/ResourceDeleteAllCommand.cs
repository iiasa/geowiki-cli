using Spectre.Console.Cli;
using GeoWiki.Cli.Services;

namespace GeoWiki.Cli.Commands.ResourceDeleteAll;

public class ResourceDeleteAllCommand : AsyncCommand
{
    private readonly ResourceService _resourceService;

    public ResourceDeleteAllCommand(ResourceService resourceService)
    {
        _resourceService = resourceService ?? throw new ArgumentNullException(nameof(resourceService));
    }

    public override async Task<int> ExecuteAsync(CommandContext context)
    {
        await _resourceService.DeleteAll();
        return 0;
    }
}