// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.CommandLine;
using System.CommandLine.Parsing;
using Microsoft.DotNet.Cli.CommandLine;
using Microsoft.DotNet.Cli.Commands.New.MSBuildEvaluation;
using Microsoft.DotNet.Cli.Commands.New.PostActions;
using Microsoft.DotNet.Cli.Commands.Workload;
using Microsoft.DotNet.Cli.Commands.Workload.List;
using Microsoft.DotNet.Cli.Extensions;
using Microsoft.DotNet.Cli.Utils;
using Microsoft.TemplateEngine.Cli;
using Microsoft.TemplateEngine.Cli.Commands;
using Command = System.CommandLine.Command;

namespace Microsoft.DotNet.Cli.Commands.New;

internal static class NewCommandDefinition
{
    public static readonly string DocsLink = "https://aka.ms/dotnet-new";
    public const string CommandName = "new";

    private const VerbosityOptions DefaultVerbosity = VerbosityOptions.normal;

    private static readonly Option<bool> s_disableSdkTemplatesOption = new Option<bool>("--debug:disable-sdk-templates")
    {
        DefaultValueFactory = static _ => false,
        Description = CliCommandStrings.DisableSdkTemplates_OptionDescription,
        Recursive = true
    }.Hide();

    private static readonly Option<bool> s_disableProjectContextEvaluationOption = new Option<bool>(
        "--debug:disable-project-context")
    {
        DefaultValueFactory = static _ => false,
        Description = CliCommandStrings.DisableProjectContextEval_OptionDescription,
        Recursive = true
    }.Hide();

    private static readonly Option<VerbosityOptions> s_verbosityOption = new("--verbosity", "-v")
    {
        DefaultValueFactory = _ => DefaultVerbosity,
        Description = CliCommandStrings.Verbosity_OptionDescription,
        HelpName = CliStrings.LevelArgumentName,
        Recursive = true
    };

    private static readonly Option<bool> s_diagnosticOption =
        CommonOptionsFactory
            .CreateDiagnosticsOption(recursive: true)
            .WithDescription(CliCommandStrings.Diagnostics_OptionDescription);

    private static Command AddOptions(Command command)
    {
        command.Options.Add(s_disableSdkTemplatesOption);
        command.Options.Add(s_disableProjectContextEvaluationOption);
        command.Options.Add(s_verbosityOption);
        command.Options.Add(s_diagnosticOption);
        return command;
    }

    public static Command Create()
        => AddOptions(NewCommandFactory.CreateDefinition(CommandName));
}
