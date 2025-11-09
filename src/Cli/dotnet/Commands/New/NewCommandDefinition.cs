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
    public const string Name = "new";
    public static readonly string DocsLink = "https://aka.ms/dotnet-new";

    public const VerbosityOptions DefaultVerbosity = VerbosityOptions.normal;

    public static readonly Option<bool> DisableSdkTemplatesOption = new Option<bool>("--debug:disable-sdk-templates")
    {
        DefaultValueFactory = static _ => false,
        Description = CliCommandStrings.DisableSdkTemplates_OptionDescription,
        Recursive = true
    }.Hide();

    public static readonly Option<bool> DisableProjectContextEvaluationOption = new Option<bool>(
        "--debug:disable-project-context")
    {
        DefaultValueFactory = static _ => false,
        Description = CliCommandStrings.DisableProjectContextEval_OptionDescription,
        Recursive = true
    }.Hide();

    public static readonly Option<VerbosityOptions> VerbosityOption = new("--verbosity", "-v")
    {
        DefaultValueFactory = _ => DefaultVerbosity,
        Description = CliCommandStrings.Verbosity_OptionDescription,
        HelpName = CliStrings.LevelArgumentName,
        Recursive = true
    };

    public static readonly Option<bool> DiagnosticOption =
        CommonOptionsFactory
            .CreateDiagnosticsOption(recursive: true)
            .WithDescription(CliCommandStrings.Diagnostics_OptionDescription);

    public static readonly IEnumerable<Option> Options =
    [
        DisableSdkTemplatesOption,
        DisableProjectContextEvaluationOption,
        VerbosityOption,
        DiagnosticOption,
    ];

    public static Command Create()
    {
        var command = NewCommandFactory.CreateDefinition(Name);
        command.Options.AddRange(Options);
        return command;
    }
}
