namespace Commands {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using DisCatSharp.CommandsNext;
    using DisCatSharp.CommandsNext.Attributes;
    using DisCatSharp.Common.Utilities;
    using DisCatSharp.Entities;
    using DisCatSharp.Interactivity;

    using Illuminate;

    using static CommandUtilities;

    internal class Commands:BaseCommandModule {
        [Command]
        private async Task Hewwo(CommandContext ctx, DiscordChannel channel) {
            if(!await IsAuthorized(ctx.Member)) return;
            
            await channel.SendMessageAsync("Hewwo OwO");
        }


        [Command]
        private async Task Say(CommandContext ctx, DiscordChannel channel) {
            if(!await IsAuthorized(ctx.Member)) return;

            string msg = ctx.Message.Content.Split(channel.Mention + " ")[1];
            await channel.SendMessageAsync(msg);
        }


        [Command]
        private async Task Beep(CommandContext ctx) {
            if(!await IsAuthorized(ctx.Member)) return;

            await ctx.Channel.SendMessageAsync("Beep");
            InteractivityResult<DiscordMessage> interaction = await Illuminate.Interactivity.WaitForMessageAsync((msg) => msg.Channel == ctx.Channel);
            if(interaction.Result is null) return;
            await ctx.Channel.SendMessageAsync($"{interaction.Result.Author.Mention} Boop");
        }


        [Command]
        private async Task Purge(CommandContext ctx, int amount = 1) {
            if(!await IsAuthorized(ctx.Member)) return;

            if(amount > 0) {
                await ctx.Message.DeleteAsync();
                amount--;
                IReadOnlyList<DiscordMessage> messages = await ctx.Channel.GetMessagesBeforeAsync(ctx.Message.Id, amount);
                foreach(DiscordMessage message in messages) {
                    await message.DeleteAsync();
                }
            }
        }


        [Command]
        private async Task Exit(CommandContext ctx) {
            if(!await IsAuthorized(ctx.Member)) return;

            await ctx.Message.DeleteAsync();
            await Illuminate.TearDownAsync();
            Environment.Exit(0);
        }
    }
}