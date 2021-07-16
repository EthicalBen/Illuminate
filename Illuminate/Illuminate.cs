namespace Illuminate {
    using System;
    using System.Threading.Tasks;

    using Commands;

    using DisCatSharp;
    using DisCatSharp.CommandsNext;
    using DisCatSharp.EventArgs;
    using DisCatSharp.Interactivity;
    using DisCatSharp.Interactivity.Extensions;

#if DEBUG
    using static Debug;
#endif

    internal static partial class Illuminate {
        internal static readonly DiscordClient Client = new DiscordClient(Config.ClientConfig);
        internal static readonly CommandsNextExtension Commands = Client.UseCommandsNext(Config.CommandsConfig);
        internal static readonly InteractivityExtension Interactivity = Client.UseInteractivity(Config.InteractivityConfig);


        private static async Task<int> Main(string[] args) {
            //---[Entrypoint]---
#if DEBUG
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
            Console.WriteLine("\n[Teardown complete. Press any kex to exit...]");
            Console.ReadKey(true);
            return 0;
        }


        private static async Task SetupAsync() {
            Client.Ready += ClientReady;
            Task connect = Client.ConnectAsync();
            Commands.RegisterCommands<BaseCommands>();
            await connect;
        }


        private static async Task ClientReady(DiscordClient sender, ReadyEventArgs e) {
            Console.WriteLine("Client Ready");
        }


        private static bool tornDown;
        internal static async Task TearDownAsync() {
            if(tornDown) return;
            tornDown = true;
            await Client.DisconnectAsync();
        }
    }
}