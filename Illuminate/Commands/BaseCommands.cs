namespace Commands {
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using DisCatSharp.CommandsNext;
    using DisCatSharp.CommandsNext.Attributes;
    using DisCatSharp.Entities;
    using DisCatSharp.Interactivity;

    using Illuminate;

    internal class BaseCommands:BaseCommandModule {


        [Command]
        private async Task Hello(CommandContext ctx) {
            await ctx.Channel.SendMessageAsync("World");
        }


        [Command]
        private async Task Purge(CommandContext ctx, int amount = 1) {
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
            await ctx.Message.DeleteAsync();
            await Illuminate.TearDownAsync();
            Environment.Exit(0);
        }


        [Command]
        private async Task Response(CommandContext ctx) {
            await ctx.Channel.SendMessageAsync("Beep");
            InteractivityResult<DiscordMessage> interaction = await Illuminate.Interactivity.WaitForMessageAsync((msg) => msg.Channel == ctx.Channel);
            await ctx.Channel.SendMessageAsync($"{interaction.Result.Author.Mention} Boop");
            Console.WriteLine($"\n\n{interaction.Result.Author.Mention}\n");
        }
    }
}