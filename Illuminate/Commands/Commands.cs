namespace Commands {
#if DEBUG || LOCAL
    using static Illuminate.Debug;
#endif

    using System.Threading.Tasks;

    using DataLayer;

    using DisCatSharp.CommandsNext;
    using DisCatSharp.CommandsNext.Attributes;
    using DisCatSharp.Entities;
    using DisCatSharp.Interactivity;

    using Illuminate;

    using static CommandUtilities;
    using System.Linq;

    internal class Commands:BaseCommandModule {
        //[Command]
        //private async Task Hewwo(CommandContext ctx, DiscordChannel channel) {
        //    if(!IsAuthorized(ctx.Member)) return;

        //    await channel.SendMessageAsync("Hewwo OwO");
        //}
    }
}