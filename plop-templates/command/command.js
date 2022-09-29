module.exports = {
    description: "New Command",
    prompts: [
        {
            type: "input",
            name: "name",
            message: "What is your command name?",
        },
    ],
    actions: function (data) {
        const actions = [];
        actions.push({
            type: "add",
            path: "GeoWiki.Cli/Commands/{{pascalCase name}}/{{pascalCase name}}Command.cs",
            templateFile: "plop-templates/command/command.cs.hbs",
        });
        actions.push({
            type: "add",
            path: "GeoWiki.Cli/Services/{{pascalCase name}}Service.cs",
            templateFile: "plop-templates/command/service.cs.hbs",
        })
        actions.push({
            type: "append",
            path: "GeoWiki.Cli/Program.cs",
            pattern: "// PLOP_SERVICE_REGISTRATION",
            template: "        registrations.AddScoped<{{pascalCase name}}Service>();",
        });

        actions.push({
            type: "append",
            path: "GeoWiki.Cli/Program.cs",
            pattern: "// PLOP_COMMAND_REGISTRATION",
            template: '            config.AddCommand<{{pascalCase name}}Command>("{{name}}");',
        })

        actions.push({
            type: "append",
            path: "GeoWiki.Cli/Program.cs",
            pattern: "// PLOP_INJECT_USING",
            template: 'using GeoWiki.Cli.Commands.{{pascalCase name}};',
        })
        return actions;
    },
};