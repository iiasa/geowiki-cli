module.exports = function generator(plop) {
    plop.setDefaultInclude({ generators: true });
    plop.setGenerator("command", require("./plop-templates/command/command.js"));
};