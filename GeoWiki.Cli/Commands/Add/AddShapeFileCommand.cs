using GeoWiki.Cli.Services;
using Spectre.Console.Cli;

namespace GeoWiki.Cli.Commands.Add;

class AddShapeFileCommand : AsyncCommand<AddSettings>
{
    private readonly ShapeFileService _shapeFileService;

    public AddShapeFileCommand(ShapeFileService shapeFileService)
    {
        _shapeFileService = shapeFileService ?? throw new ArgumentNullException(nameof(shapeFileService));
    }

    public override async Task<int> ExecuteAsync(CommandContext context, AddSettings settings)
    {
        await _shapeFileService.AddShapeFileAsync(settings.FilePath);
        return 0;
    }
}