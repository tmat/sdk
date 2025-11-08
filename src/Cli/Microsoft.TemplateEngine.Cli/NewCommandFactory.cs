// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.CommandLine;
using Microsoft.TemplateEngine.Cli.Commands;

namespace Microsoft.TemplateEngine.Cli
{
    public static class NewCommandFactory
    {
        public static Command Create(string commandName, Func<ParseResult, ICliTemplateEngineHost> hostBuilder)
        {
            ArgumentNullException.ThrowIfNull(hostBuilder);

            var command = CreateDefinitionImpl(commandName);
            command.SetAction(hostBuilder);
            return command;
        }

        public static Command CreateDefinition(string commandName)
            => CreateDefinitionImpl(commandName);

        internal static NewCommand CreateDefinitionImpl(string commandName)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(commandName, nameof(commandName));
            return new NewCommand(commandName);
        }
    }
}
