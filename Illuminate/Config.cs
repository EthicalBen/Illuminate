namespace Illuminate {
    using System;

    using DisCatSharp;
    using DisCatSharp.ApplicationCommands;
    using DisCatSharp.CommandsNext;
    using DisCatSharp.Interactivity;

    internal static class Config {
        internal static readonly DiscordConfiguration ClientConfig = new DiscordConfiguration {
            Intents = DiscordIntents.All,
#if DEBUG
            MinimumLogLevel = Microsoft.Extensions.Logging.LogLevel.Debug,
#else
            MinimumLogLevel = Microsoft.Extensions.Logging.LogLevel.Information,
#endif
            Token = Secrets.Token
        };


        internal static readonly CommandsNextConfiguration CommandsConfig = new CommandsNextConfiguration {
            CaseSensitive = true,
            EnableDefaultHelp = false,
            EnableDms = false,
            EnableMentionPrefix = false,
            IgnoreExtraArguments = true,
            StringPrefixes = new string[] {
                ">"
            }
        };


        internal static readonly InteractivityConfiguration InteractivityConfig = new InteractivityConfiguration {
            Timeout = TimeSpan.FromMinutes(5)
        };


        internal static readonly ApplicationCommandsConfiguration ApplicationCommandsConfig = new ApplicationCommandsConfiguration { };
    }
}