// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.CommandLine;
using System.CommandLine.Completions;
using Microsoft.DotNet.Cli.CommandLine;
using Microsoft.TemplateEngine.Abstractions;
using Microsoft.TemplateEngine.Edge.Settings;

namespace Microsoft.TemplateEngine.Cli.Commands
{
    internal partial class NewCommand : BaseCommand<NewCommandArgs>, ICustomHelp
    {
        internal NewCommand(string commandName)
            : base(commandName, SymbolStrings.Command_New_Description)
        {
            this.DocsLink = "https://aka.ms/dotnet-new";
            TreatUnmatchedTokensAsErrors = true;

            //it is important that legacy commands are built before non-legacy, as non legacy commands are building validators that rely on legacy stuff
            BuildLegacySymbols();

            Add(new InstantiateCommand(this));
            Add(new InstallCommand(this));
            Add(new UninstallCommand(this));
            Add(new UpdateCommand(this));
            Add(new SearchCommand(this));
            Add(new ListCommand(this));
            Add(new AliasCommand());
            Add(new DetailsCommand());

            Options.Add(DebugCustomSettingsLocationOption);
            Options.Add(DebugVirtualizeSettingsOption);
            Options.Add(DebugAttachOption);
            Options.Add(DebugReinitOption);
            Options.Add(DebugRebuildCacheOption);
            Options.Add(DebugShowConfigOption);

            Options.Add(SharedOptions.OutputOption);
            Options.Add(SharedOptions.NameOption);
            Options.Add(SharedOptions.DryRunOption);
            Options.Add(SharedOptions.ForceOption);
            Options.Add(SharedOptions.NoUpdateCheckOption);
            Options.Add(SharedOptions.ProjectPathOption);
        }

        internal static Option<string?> DebugCustomSettingsLocationOption { get; } = new("--debug:custom-hive")
        {
            Description = SymbolStrings.Option_Debug_CustomSettings,
            Hidden = true,
            Recursive = true
        };

        internal static Option<bool> DebugVirtualizeSettingsOption { get; } = new("--debug:ephemeral-hive", "--debug:virtual-hive")
        {
            Description = SymbolStrings.Option_Debug_VirtualSettings,
            Hidden = true,
            Recursive = true
        };

        internal static Option<bool> DebugAttachOption { get; } = new("--debug:attach")
        {
            Description = SymbolStrings.Option_Debug_Attach,
            Hidden = true,
            Recursive = true
        };

        internal static Option<bool> DebugReinitOption { get; } = new("--debug:reinit")
        {
            Description = SymbolStrings.Option_Debug_Reinit,
            Hidden = true,
            Recursive = true
        };

        internal static Option<bool> DebugRebuildCacheOption { get; } = new("--debug:rebuild-cache", "--debug:rebuildcache")
        {
            Description = SymbolStrings.Option_Debug_RebuildCache,
            Hidden = true,
            Recursive = true
        };

        internal static Option<bool> DebugShowConfigOption { get; } = new("--debug:show-config", "--debug:showconfig")
        {
            Description = SymbolStrings.Option_Debug_ShowConfig,
            Hidden = true,
            Recursive = true
        };

        internal static Argument<string> ShortNameArgument { get; } = new("template-short-name")
        {
            Description = SymbolStrings.Command_Instantiate_Argument_ShortName,
            Arity = new ArgumentArity(0, 1),
            Hidden = true
        };

        internal static Argument<string[]> RemainingArguments { get; } = new("template-args")
        {
            Description = SymbolStrings.Command_Instantiate_Argument_TemplateOptions,
            Arity = new ArgumentArity(0, 999),
            Hidden = true
        };

        internal IReadOnlyList<Option> PassByOptions { get; } = new Option[]
        {
            SharedOptions.ForceOption,
            SharedOptions.NameOption,
            SharedOptions.DryRunOption,
            SharedOptions.NoUpdateCheckOption
        };

        public override void SetAction(Func<ParseResult, ITemplateEngineHost> hostBuilder)
        {
            base.SetAction(hostBuilder);

            foreach (BaseCommand command in Subcommands)
            {
                command.SetAction(hostBuilder);
            }
        }

        protected internal override IEnumerable<CompletionItem> GetCompletions(CompletionContext context, IEngineEnvironmentSettings environmentSettings, TemplatePackageManager templatePackageManager)
        {
            if (context is not TextCompletionContext textCompletionContext)
            {
                foreach (CompletionItem completion in base.GetCompletions(context, environmentSettings, templatePackageManager))
                {
                    yield return completion;
                }
                yield break;
            }

            InstantiateCommandArgs instantiateCommandArgs = InstantiateCommandArgs.FromNewCommandArgs(ParseContext(context.ParseResult));
            HostSpecificDataLoader? hostSpecificDataLoader = new(environmentSettings);

            //TODO: consider new API to get templates only from cache (non async)
            IReadOnlyList<ITemplateInfo> templates =
                Task.Run(async () => await templatePackageManager.GetTemplatesAsync(default).ConfigureAwait(false)).GetAwaiter().GetResult();

            IEnumerable<TemplateGroup> templateGroups = TemplateGroup.FromTemplateList(CliTemplateInfo.FromTemplateInfo(templates, hostSpecificDataLoader));

            if (templateGroups.Any(template => template.ShortNames.Contains(instantiateCommandArgs.ShortName)))
            {
                foreach (CompletionItem completion in InstantiateCommand.GetTemplateCompletions(instantiateCommandArgs, templateGroups, environmentSettings, templatePackageManager, textCompletionContext))
                {
                    yield return completion;
                }
                yield break;
            }

            foreach (CompletionItem completion in InstantiateCommand.GetTemplateNameCompletions(instantiateCommandArgs.ShortName, templateGroups, environmentSettings))
            {
                yield return completion;
            }
            foreach (CompletionItem completion in base.GetCompletions(context, environmentSettings, templatePackageManager))
            {
                yield return completion;
            }
        }

        protected override Task<NewCommandStatus> ExecuteAsync(
            NewCommandArgs args,
            IEngineEnvironmentSettings environmentSettings,
            TemplatePackageManager templatePackageManager,
            ParseResult parseResult,
            CancellationToken cancellationToken)
        {
            return InstantiateCommand.ExecuteAsync(args, environmentSettings, templatePackageManager, parseResult, cancellationToken);
        }

        protected override NewCommandArgs ParseContext(ParseResult parseResult) => new(this, parseResult);
    }
}

