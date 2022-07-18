using GeoWiki.Cli.Services;
using Spectre.Console.Cli;

namespace GeoWiki.Cli.Commands.AddShapeFile;

class AddShapeFileCommand : AsyncCommand<AddShapeFileSettings>
{
    private readonly ShapeFileService _shapeFileService;

    public AddShapeFileCommand(ShapeFileService shapeFileService)
    {
        _shapeFileService = shapeFileService ?? throw new ArgumentNullException(nameof(shapeFileService));
    }

    public override async Task<int> ExecuteAsync(CommandContext context, AddShapeFileSettings settings)
    {
        await _shapeFileService.AddShapeFileAsync(settings.FilePath, settings.TableName);
        return 0;
    }
}