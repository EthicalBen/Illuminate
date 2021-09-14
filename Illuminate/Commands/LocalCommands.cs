namespace Commands {
    using System.Threading.Tasks;

    using DataLayer;

    using DisCatSharp.CommandsNext;
    using DisCatSharp.CommandsNext.Attributes;
    using DisCatSharp.Entities;

    using System.Linq;

    using static CommandUtilities;
    using System;
#if DEBUG || LOCAL
    using static Illuminate.Debug;
    using Illuminate;
    using System.Collections.Generic;
    using Microsoft.EntityFrameworkCore;
    using DisCatSharp;
#endif
    internal class LocalCommands:BaseCommandModule {

        [Command]
        private async Task Test(CommandContext ctx) {
            if(!await IsAuthorized(ctx.Member)) return;

            IlluminateContext ictx = new();

            try {
                ictx.Members.Add((EDiscordMember)ctx.Member);
                ictx.SaveChanges();
                await ctx.Channel.SendMessageAsync($"Response: {ictx.Members.Single().Id}");
            } catch(Exception e) { LogToSource(ctx, e); }
        }


        [Command]
        private async Task AddSwearWord(CommandContext ctx, params string[] words) {

            if(!await IsAuthorized(ctx.Member)) return;

            string word = words.Aggregate("", (last, next) => last + next);

            IlluminateContext ictx = new();

            word = word.Trim('|').ToLowerInvariant();

            try {
                ictx.SwearWords.Add(new(word));
                ictx.SaveChanges();
                await ctx.Channel.SendMessageAsync($"Word added: ||{word}||");
            } catch(Exception e) { LogToSource(ctx, e); }
        }


        [Command]
        private async Task RemoveSwearWord(CommandContext ctx, params string[] words) {
            string word = words.Aggregate("", (last, next) => last + next);
            if(!await IsAuthorized(ctx.Member)) return;

            IlluminateContext ictx = new();

            word = word.Trim('|');

            try{
                EString badWord = ictx.SwearWords.FirstOrDefault((estring) => estring.String == word.ToLowerInvariant());
                if(badWord is not null) {
                    ictx.SwearWords.Remove(badWord);
                    ictx.SaveChanges();
                    await ctx.Channel.SendMessageAsync($"Word removed: ||{word}||");
                } else {
                    await ctx.Channel.SendMessageAsync($"Word not found: ||{word}||");
                }
            } catch(Exception e) { LogToSource(ctx, e); }
        }

        [Command]
        private async Task SwearWords(CommandContext ctx, string word) {
            if(!await IsAuthorized(ctx.Member)) return;

            IlluminateContext ictx = new();

            try {
                string words = "";
                await ictx.SwearWords.ForEachAsync((badWord) => words += "||" + badWord.String + "||");
                await ctx.Channel.SendMessageAsync($"Words: {words}");
            } catch(Exception e) { LogToSource(ctx, e); }
        }


        [Command]
        private async Task ResetSwearWords(CommandContext ctx) {
            if(!await IsAuthorized(ctx.Member)) return;

            IlluminateContext ictx = new();

            try{
                string words = "";
                await ictx.SwearWords.ForEachAsync((badWord) => {
                    ictx.Remove(badWord);
                    words += "||" + badWord.String + "||";
                });
                ictx.SaveChanges();
                await ctx.Channel.SendMessageAsync($"Words removed: {words}");
            } catch(Exception e) { LogToSource(ctx, e); }
        }

        [Command]
        private async Task TryAddGuildCommand(CommandContext ctx) {
            if(!await IsAuthorized(ctx.Member)) return;
            try {
                DiscordClient clt = Illuminate.Client;
                IReadOnlyList<DiscordApplicationCommand> cmds = await clt.GetGuildApplicationCommandsAsync(ctx.Guild.Id);
                string name = "command";
                DiscordApplicationCommand cmd = cmds.FirstOrDefault((cmd) => cmd.Name == name);
                if(cmd is null) {
                    await clt.CreateGlobalApplicationCommandAsync(
                        new DiscordApplicationCommand(
                            name,
                            "Command creation tool",
                            new List<DiscordApplicationCommandOption>(){
                                new(
                                    "name",
                                    "Name of the command",
                                    ApplicationCommandOptionType.String,
                                    true
                                    )
                            },
                            true)
                        );
                    await ctx.Channel.SendMessageAsync("An attempt was made.");
                }
            } catch(Exception e) { LogToSource(ctx, e); }
        }


        [Command]
        private async Task MassPurge(CommandContext ctx, int amount = 1) {
            if(!await IsAuthorized(ctx.Member)) return;

            if(amount > 0) {
                try {
                    await ctx.Channel.DeleteMessagesAsync(await ctx.Channel.GetMessagesBeforeAsync(ctx.Message.Id, amount));
                } catch(Exception e) {
                    LogToSource(ctx, e);
                } finally {
                    await ctx.Channel.DeleteMessageAsync(ctx.Message);
                }
            }
        }
    }
}