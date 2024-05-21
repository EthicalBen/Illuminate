
namespace Illuminate;

using System;
using System.Threading.Tasks;

using DisCatSharp;
using DisCatSharp.CommandsNext;
using DisCatSharp.EventArgs;
using DisCatSharp.Interactivity;
using DisCatSharp.Interactivity.Extensions;
using DisCatSharp.ApplicationCommands;

using Services;

internal static class Illuminate
{
    internal static readonly DiscordClient Client = new(Config.ClientConfig);
    internal static readonly CommandsNextExtension Commands = Client.UseCommandsNext(Config.CommandsConfig);
    internal static readonly InteractivityExtension Interactivity = Client.UseInteractivity(Config.InteractivityConfig);
    internal static readonly ApplicationCommandsExtension ApplicationCommands = Client.UseApplicationCommands(Config.ApplicationCommandsConfig);

    private static async Task<int> Main(string[] args)
    {
        //---[Entrypoint]---
        await SetupAsync();


        //---[Exitpoint]---
        while (Console.KeyAvailable) Console.ReadKey(true);
        Console.WriteLine("\n[Press any key to start teardown...]");
        Console.ReadKey(true);
        await TearDownAsync();
        while (Console.KeyAvailable) Console.ReadKey(true);
        Console.WriteLine("\n[Teardown complete. Press any key to exit...]");
        Console.ReadKey(true);
        return 0;
    }

    private static async Task SetupAsync()
    {
        Client.Ready += ClientReady;
        await Client.ConnectAsync();
    }


    private static Task ClientReady(DiscordClient client, ReadyEventArgs e)
    {
        Console.WriteLine("Client Ready");

        Services.RegisterServices(client);

        return Task.CompletedTask;
    }


    private static bool tornDown;
    internal static async Task TearDownAsync()
    {
        if (tornDown) return;

        tornDown = true;
        await Client.DisconnectAsync();
    }
}