// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.TemplateEngine.Cli.Commands
{
    internal class LegacyAliasShowCommand : BaseAliasShowCommand
    {
        internal LegacyAliasShowCommand()
            : base("--show-alias")
        {
            Hidden = true;
        }
    }
}
