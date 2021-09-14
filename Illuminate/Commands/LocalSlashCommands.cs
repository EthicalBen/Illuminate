namespace Commands {
    using System.Threading.Tasks;

    using DataLayer;

    using System.Linq;
    using DisCatSharp;
    using DisCatSharp.Entities;

    using static CommandUtilities;
    using System;
#if DEBUG || LOCAL
    using static Illuminate.Debug;
#endif
    using DisCatSharp.SlashCommands;
    internal class LocalSlashCommands:SlashCommandModule {

        [SlashCommand("command", "Command")]
        private async Task Command(InteractionContext ctx, [Option("test","test")] string name) {
            if(!await IsAuthorized(ctx.Member)) return;

            IlluminateContext ictx = new();

            _(ctx.Interaction.Type);

            try {
                DiscordInteractionResponseBuilder builder = new();
                builder.IsEphemeral = true;
                builder.Content = "Test";
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, builder);
            } catch(Exception e) { LogToSource(ctx, e); }
        }
    }
}