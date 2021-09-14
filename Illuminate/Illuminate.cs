namespace Illuminate {
    using System;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;

    using Commands;

    using DataLayer;

    using DisCatSharp;
    using DisCatSharp.CommandsNext;
    using DisCatSharp.EventArgs;
    using DisCatSharp.Interactivity;
    using DisCatSharp.Interactivity.Extensions;
    using DisCatSharp.SlashCommands;

    using Services;

#if DEBUG
    using static Debug;
#endif

    internal static class Illuminate{
        internal static readonly DiscordClient Client = new DiscordClient(Config.ClientConfig);
        internal static readonly CommandsNextExtension Commands = Client.UseCommandsNext(Config.CommandsConfig);
        internal static readonly InteractivityExtension Interactivity = Client.UseInteractivity(Config.InteractivityConfig);
        internal static readonly SlashCommandsExtension SlashCommands = Client.UseSlashCommands(Config.SlashCommandsConfig);

        private static async Task<int> Main(string[] args) {
            //---[Entrypoint]---
#if LOCAL
            _("LOCAL Build!");
#elif DEBUG
            _("DEBUG Build!");
            //await AsyncBreakfast.AsyncBreakfast.Main(args);
#endif
            await SetupAsync();

            //---[Exitpoint]---
            while(Console.KeyAvailable) Console.ReadKey(true);
            Console.WriteLine("\n[Press any key to start teardown...]");
            Console.ReadKey(true);
            await TearDownAsync();
            while(Console.KeyAvailable) Console.ReadKey(true);
            Console.WriteLine("\n[Teardown complete. Press any key to exit...]");
            Console.ReadKey(true);
            return 0;
        }


        private static async Task SetupAsync() { // redo this, add disposable, add resetevent, add timeout, https://stackoverflow.com/questions/4989626/wait-until-a-delegate-is-called
            Client.Ready += ClientReady;
            Task connect = Client.ConnectAsync();
#if LOCAL
            SlashCommands.RegisterCommands<LocalSlashCommands>();
            Commands.RegisterCommands<LocalCommands>();
#else
            Commands.RegisterCommands<Commands>();
#endif
            await connect;
        }


        private static Task ClientReady(DiscordClient sender, ReadyEventArgs e) {
            Console.WriteLine("Client Ready");

            Services.RegisterServices(sender);

            return Task.CompletedTask;
        }


        private static bool tornDown;
        internal static async Task TearDownAsync() {
            if(tornDown) return;

            await Client.DisconnectAsync();
            tornDown = true;
        }
    }
}