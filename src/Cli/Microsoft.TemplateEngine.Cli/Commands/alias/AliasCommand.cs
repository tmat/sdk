// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.CommandLine;
using Microsoft.TemplateEngine.Abstractions;
using Microsoft.TemplateEngine.Edge.Settings;

namespace Microsoft.TemplateEngine.Cli.Commands
{
    internal class AliasCommand : BaseCommand<AliasCommandArgs>
    {
        internal AliasCommand()
            : base("alias", SymbolStrings.Command_Alias_Description)
        {
            Hidden = true;
            Add(new AliasAddCommand());
            Add(new AliasShowCommand());
        }

        protected override Task<NewCommandStatus> ExecuteAsync(
            AliasCommandArgs args,
            IEngineEnvironmentSettings environmentSettings,
            TemplatePackageManager templatePackageManager,
            ParseResult parseResult,
            CancellationToken cancellationToken) => throw new NotImplementedException();

        protected override AliasCommandArgs ParseContext(ParseResult parseResult) => new(this, parseResult);
    }
}
