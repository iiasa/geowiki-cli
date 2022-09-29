using System;
using System.ComponentModel;
using Spectre.Console.Cli;
namespace GeoWiki.Cli.Commands.Default;

public class DefaultCommand : Command<DefaultCommand.Settings>
{
    private readonly IGreeter _greeter;

    public sealed class Settings : CommandSettings
    {
        [CommandOption("-n|--name <NAME>")]
        [Description("The person or thing to greet.")]
        [DefaultValue("World")]
        public string Name { get; set; } = "World";
    }

    public DefaultCommand(IGreeter greeter)
    {
        _greeter = greeter ?? throw new ArgumentNullException(nameof(greeter));
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        _greeter.Greet(settings.Name);
        return 0;
    }
}