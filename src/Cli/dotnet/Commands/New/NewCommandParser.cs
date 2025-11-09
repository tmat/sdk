// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.CommandLine;
using System.CommandLine.Parsing;
using Microsoft.DotNet.Cli.Commands.New.MSBuildEvaluation;
using Microsoft.DotNet.Cli.Commands.New.PostActions;
using Microsoft.DotNet.Cli.Commands.Workload;
using Microsoft.DotNet.Cli.Commands.Workload.List;
using Microsoft.DotNet.Cli.Extensions;
using Microsoft.DotNet.Cli.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.TemplateEngine.Abstractions;
using Microsoft.TemplateEngine.Abstractions.Components;
using Microsoft.TemplateEngine.Abstractions.Constraints;
using Microsoft.TemplateEngine.Abstractions.TemplatePackage;
using Microsoft.TemplateEngine.Cli;
using Microsoft.TemplateEngine.Cli.Commands;
using Microsoft.TemplateEngine.Cli.PostActionProcessors;
using Command = System.CommandLine.Command;

namespace Microsoft.DotNet.Cli.Commands.New;

internal static class NewCommandParser
{
    private const string EnableProjectContextEvaluationEnvVarName = "DOTNET_CLI_DISABLE_PROJECT_EVAL";
    private const string PrefferedLangEnvVarName = "DOTNET_NEW_PREFERRED_LANG";

    private const string HostIdentifier = "dotnetcli";

    internal static readonly Command s_command = GetCommand();

    public static Command GetCommand()
    {
        var command = NewCommandFactory.Create(NewCommandDefinition.Name, GetEngineHost);
        command.Options.AddRange(NewCommandDefinition.Options);
        return command;

        static CliTemplateEngineHost GetEngineHost(ParseResult parseResult)
        {
            bool disableSdkTemplates = parseResult.GetValue(NewCommandDefinition.DisableSdkTemplatesOption);
            bool disableProjectContext = parseResult.GetValue(NewCommandDefinition.DisableProjectContextEvaluationOption)
                || Env.GetEnvironmentVariableAsBool(EnableProjectContextEvaluationEnvVarName);
            bool diagnosticMode = parseResult.GetValue(NewCommandDefinition.DiagnosticOption);
            FileInfo? projectPath = parseResult.GetValue(SharedOptions.ProjectPathOption);
            FileInfo? outputPath = parseResult.GetValue(SharedOptions.OutputOption);

            OptionResult? verbosityOptionResult = parseResult.GetResult(NewCommandDefinition.VerbosityOption);
            VerbosityOptions verbosity = NewCommandDefinition.DefaultVerbosity;

            if (diagnosticMode || CommandLoggingContext.IsVerbose)
            {
                CommandLoggingContext.SetError(true);
                CommandLoggingContext.SetOutput(true);
                CommandLoggingContext.SetVerbose(true);
                verbosity = VerbosityOptions.diagnostic;
            }
            else if (verbosityOptionResult != null
                && !verbosityOptionResult.Implicit
                // if verbosityOptionResult contains an error, ArgumentConverter.GetValueOrDefault throws an exception
                // and callstack is pushed to process output
                && !parseResult.Errors.Any(error => error.SymbolResult == verbosityOptionResult))
            {
                VerbosityOptions userSetVerbosity = verbosityOptionResult.GetValueOrDefault<VerbosityOptions>();
                if (userSetVerbosity.IsQuiet())
                {
                    CommandLoggingContext.SetError(false);
                    CommandLoggingContext.SetOutput(false);
                    CommandLoggingContext.SetVerbose(false);
                }
                else if (userSetVerbosity.IsMinimal())
                {
                    CommandLoggingContext.SetError(true);
                    CommandLoggingContext.SetOutput(false);
                    CommandLoggingContext.SetVerbose(false);
                }
                else if (userSetVerbosity.IsNormal())
                {
                    CommandLoggingContext.SetError(true);
                    CommandLoggingContext.SetOutput(true);
                    CommandLoggingContext.SetVerbose(false);
                }
                verbosity = userSetVerbosity;
            }
            Reporter.Reset();
            return CreateHost(disableSdkTemplates, disableProjectContext, projectPath, outputPath, parseResult, verbosity.ToLogLevel());
        }
    }

    private static CliTemplateEngineHost CreateHost(
        bool disableSdkTemplates,
        bool disableProjectContext,
        FileInfo? projectPath,
        FileInfo? outputPath,
        ParseResult parseResult,
        LogLevel logLevel)
    {
        var builtIns = new List<(Type InterfaceType, IIdentifiedComponent Instance)>();
        builtIns.AddRange(TemplateEngine.Orchestrator.RunnableProjects.Components.AllComponents);
        builtIns.AddRange(TemplateEngine.Edge.Components.AllComponents);
        builtIns.AddRange(Components.AllComponents);
        builtIns.AddRange(TemplateSearch.Common.Components.AllComponents);

        //post actions
        builtIns.AddRange(
        [
            (typeof(IPostActionProcessor), new DotnetAddPostActionProcessor()),
            (typeof(IPostActionProcessor), new DotnetSlnPostActionProcessor()),
            (typeof(IPostActionProcessor), new DotnetRestorePostActionProcessor())
        ]);
        if (!disableSdkTemplates)
        {
            builtIns.Add((typeof(ITemplatePackageProviderFactory), new BuiltInTemplatePackageProviderFactory()));
            builtIns.Add((typeof(ITemplatePackageProviderFactory), new OptionalWorkloadProviderFactory()));
        }
        if (!disableProjectContext)
        {
            builtIns.Add((typeof(IBindSymbolSource), new ProjectContextSymbolSource()));
            builtIns.Add((typeof(ITemplateConstraintFactory), new ProjectCapabilityConstraintFactory()));
            builtIns.Add((typeof(MSBuildEvaluator), new MSBuildEvaluator(outputDirectory: outputPath?.FullName, projectPath: projectPath?.FullName)));
        }

        builtIns.Add((typeof(IWorkloadsInfoProvider), new WorkloadsInfoProvider(
                new Lazy<IWorkloadsRepositoryEnumerator>(() => new WorkloadInfoHelper(parseResult.HasOption(SharedOptions.InteractiveOption)))))
        );
        builtIns.Add((typeof(ISdkInfoProvider), new SdkInfoProvider()));

        string? preferredLangEnvVar = Environment.GetEnvironmentVariable(PrefferedLangEnvVarName);
        string preferredLang = string.IsNullOrWhiteSpace(preferredLangEnvVar) ? "C#" : preferredLangEnvVar;

        var preferences = new Dictionary<string, string>
        {
            { "prefs:language", preferredLang },
            { "dotnet-cli-version", Product.Version },
            { "RuntimeFrameworkVersion", new Muxer().SharedFxVersion },
            { "NetStandardImplicitPackageVersion", new FrameworkDependencyFile().GetNetStandardLibraryVersion() ?? "" },
        };
        return new CliTemplateEngineHost(
            HostIdentifier,
            Product.Version,
            preferences,
            builtIns,
            outputPath: outputPath?.FullName,
            logLevel: logLevel);
    }
}
