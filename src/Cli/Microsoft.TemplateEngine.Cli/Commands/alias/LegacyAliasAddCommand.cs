// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.TemplateEngine.Cli.Commands
{
    internal class LegacyAliasAddCommand : BaseAliasAddCommand
    {
        internal LegacyAliasAddCommand()
            : base("--alias")
        {
            Aliases.Add("-a");
            Hidden = true;
        }
    }
}
