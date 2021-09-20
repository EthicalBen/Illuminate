namespace Commands {
#if DEBUG || LOCAL
    using static Illuminate.Debug;
#endif
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using DataLayer;

    using DisCatSharp;
    using DisCatSharp.Entities;
    using DisCatSharp.Interactivity;
    using DisCatSharp.SlashCommands;

    using Illuminate;

    using static CommandUtilities;
    using DisCatSharp.CommandsNext.Converters;
    using System.Collections.Generic;
    using Microsoft.EntityFrameworkCore;
    using System.Security.Cryptography.X509Certificates;
    using System.Reflection.Metadata.Ecma335;
    using System.Net.Http;
using Illuminate.Migrations;

    internal class SlashCommands:SlashCommandModule {

        [SlashCommandGroup("swear-words", "The swear words module", true)]
        private class SwearWords {
            [SlashCommand("add", "Add a swear word")]
            private async Task Add(InteractionContext ctx, [Option("word", "The word to add")] string word) {
#if !LOCAL
                if(!IsAuthorized(ctx.Member)) return;

                word = word.Trim('|').ToLowerInvariant();

                try {
                    Ictx.SwearWords.Add(new(word));
                    Ictx.SaveChanges();
                    await ctx.CreateResponseAsync($"Word added: ||{word}||");
                } catch(Exception e) { await LogToSource(ctx, e); }
#endif
            }


            [SlashCommand("remove", "Remove a swear word")]
            private async Task Remove(InteractionContext ctx, [Option("word", "The word to remove")] string word) {
#if !LOCAL
                if(!IsAuthorized(ctx.Member)) return;

                word = word.Trim('|').ToLowerInvariant();

                try {
                    EString badWord = Ictx.SwearWords.FirstOrDefault((estring) => estring.String == word);
                    string response;
                    if(badWord is not null) {
                        Ictx.SwearWords.Remove(badWord);
                        Ictx.SaveChanges();
                        response = $"Word removed: ||{word}||";
                    } else {
                        response = $"Word not found: ||{word}||";
                    }
                    await ctx.CreateResponseAsync(response);
                } catch(Exception e) { await LogToSource(ctx, e); }
#endif
            }


            [SlashCommand("list", "List swear words")]
            private async Task List(InteractionContext ctx) {
#if !LOCAL
                if(!IsAuthorized(ctx.Member)) return;

                try {
                    string response;
                    if(Ictx.SwearWords.Count() != 0) {
                        response = "Words: ";
                        foreach(EString word in Ictx.SwearWords) {
                            response += $"||{word.String}||";
                        }
                    } else {
                        response = "No words in database";
                    }
                    await ctx.CreateResponseAsync(response);
                } catch(Exception e) { await LogToSource(ctx, e); }
#endif
            }


            [SlashCommand("reset", "clear swear words")]
            private async Task ResetSwearWords(InteractionContext ctx) {
#if !LOCAL
                if(!IsAuthorized(ctx.Member)) return;

                try {
                    string response;
                    if(Ictx.SwearWords.Count() != 0) {
                        response = "Words removed: ";
                        foreach(EString word in Ictx.SwearWords) {
                            response += $"||{word.String}||";
                            Ictx.SwearWords.Remove(word);
                        }
                        Ictx.SaveChanges();
                    } else {
                        response = "No words in database";
                    }
                    await ctx.CreateResponseAsync(response);
                } catch(Exception e) { await LogToSource(ctx, e); }
#endif
            }
        }


        [SlashCommand("purge", "purges messages from a channel")]
        private async Task Purge(InteractionContext ctx, [Option("amount", "How many messages to remove")] long amount = 1) {
#if !LOCAL
            if(!IsAuthorized(ctx.Member)) return;

            if(amount > 0) {
                try {
                    await ctx.Channel.DeleteMessagesAsync(await ctx.Channel.GetMessagesBeforeAsync(ctx.Channel.LastMessageId.Value, (int)amount));
                } catch(Exception e) {
                    await LogToSource(ctx, e);
                } finally {
                    await ctx.CreateResponseAsync($"Purged {amount} message{(amount > 1 ? "s" : "")}");
                }
            }
#endif
        }


        [SlashCommand("exit", "Stops the bot")]
        private async Task Exit(InteractionContext ctx) {
            if(!IsAuthorized(ctx.Member)) return;

            await Illuminate.TearDownAsync();
            Environment.Exit(0);
        }


        [SlashCommand("beep", "Boop the next person to send a message here")]
        private async Task Beep(InteractionContext ctx) {
#if !LOCAL
            if(!IsAuthorized(ctx.Member)) return;

            await ctx.CreateResponseAsync(
                InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder()
                .WithContent("Beep")
                .AsEphemeral(false)
            );
            InteractivityResult<DiscordMessage> interaction = await Illuminate.Interactivity.WaitForMessageAsync((msg) => msg.Channel == ctx.Channel);
            if(interaction.Result is null) return;
            await ctx.Channel.SendMessageAsync($"{interaction.Result.Author.Mention} Boop");
#endif
        }


        [SlashCommand("say", "Make Illuminate say something")]
        private async Task Say(
            InteractionContext ctx,

            [Option("channel", "where")]
            DiscordChannel channel = null,

            [Option("message", "what")]
            string message = "<3"
        ) {
#if !LOCAL
            if(!IsAuthorized(ctx.Member)) return;

            channel ??= ctx.Channel;

            await ctx.CreateResponseAsync("Sending message...");
            try {
                await channel.SendMessageAsync(message);
            } catch (Exception e) { await LogToSource(ctx, e); }
#endif
        }


        [SlashCommandGroup("react-role", "The reaction roles module")]
        private class ReactRoles {

            [SlashCommand("add", "Add a reaction role to a draft or message")]
            private async Task Add(
                InteractionContext ctx,

                [Option("tag", "Tag of the draft or message")]
                string tag,

                [Option("role", "What role")]
                DiscordRole role,

                [Option("emoji", "What emoji")]
                string rawEmoji
            ) {
                if(!IsAuthorized(ctx.Member)) return;

                try {
                    await ctx.DeferResponseAsync();

                    if(role == ctx.Guild.EveryoneRole || role.IsManaged ||
                        (await ctx.Guild.GetMemberAsync(Illuminate.Client.CurrentUser.Id))
                        .Roles.Aggregate<DiscordRole, int?>(
                            null,
                            (last, next) => last is null ? next.Position : next.Position > last ? next.Position : last) < role.Position) {
                        await ctx.EditResponseAsync($"Not an assignable role.");
                        return;
                    }

                    rawEmoji = rawEmoji.Trim();

                    DiscordEmoji emoji =
                        DiscordEmoji.TryFromUnicode(rawEmoji, out DiscordEmoji result)
                        ? result
                        : DiscordEmoji.TryFromName(Illuminate.Client, rawEmoji, true, out result)
                        ? result
                        : null;

                    if(emoji is null) {
                        await ctx.EditResponseAsync("Emote not valid or out of reach.");
                        return;
                    }

                    EReactionMessage reactionMessage =
                        Ictx.ReactionMessages
                        .Include(x => x.Pairs)
                        .FirstOrDefault(x => x.Tag == tag);

                    if(reactionMessage is null) {
                        await ctx.EditResponseAsync("Tag not found.");
                        return;
                    }

                    EmojiRolePair existingPair = reactionMessage.Pairs.FirstOrDefault(p => p.RoleId == role.Id);
                    if(existingPair is not null) {
                        if(existingPair.EmojiName == emoji.GetDiscordName()) {
                            await ctx.EditResponseAsync("Nothing to add.");
                            return;
                        }

                        existingPair.EmojiName = emoji.GetDiscordName();
                        Ictx.SaveChanges();
                        await UpdateReactionMessage(ctx, reactionMessage);

                        await ctx.EditResponseAsync($"Updated Emoji for existing role.");
                        return;
                    }

                    reactionMessage.Pairs.Add(new() { EmojiName = emoji.GetDiscordName(), RoleId = role.Id });
                    Ictx.SaveChanges();//reactionMessage.Pairs = reactionMessage.Pairs; // Really tho?
                    await UpdateReactionMessage(ctx, reactionMessage);

                    await ctx.EditResponseAsync($"Added {role.Mention} with reaction {emoji} to \"{tag}\"");

                    await ctx.FollowUpAsync("[Preview]\n" + CreateMessageBody(ctx, reactionMessage));
                } catch(Exception e) { await LogToSource(ctx, e); }
            }


            private static async Task UpdateReactionMessage(InteractionContext ctx, EReactionMessage reactionMessage) {
                if(reactionMessage.MessageId is not null) {
                    DiscordMessage message =
                        await Illuminate.Client
                        .GetChannelAsync((ulong)reactionMessage.ChannelId)
                        .ContinueWith(t => t.Result.GetMessageAsync((ulong)reactionMessage.MessageId).Result);
                    await message.DeleteAllReactionsAsync();


                    foreach(EmojiRolePair pair in reactionMessage.Pairs)
                        await message.CreateReactionAsync(DiscordEmoji.FromName(Illuminate.Client, pair.EmojiName));

                    string content = CreateMessageBody(ctx, reactionMessage);
                    await message.ModifyAsync(content);
                }
            }


            private static string CreateMessageBody(InteractionContext ctx, EReactionMessage reactionMessage) {
                string content = reactionMessage.Content;
                foreach(EmojiRolePair pair in reactionMessage.Pairs) {
                    content += $"\n{pair.EmojiName}: {ctx.Channel.Guild.GetRole(pair.RoleId).Mention}";
                }

                return content;
            }


            [SlashCommand("create", "Create a reaction role draft")]
            private async Task Create(
                InteractionContext ctx,

                [Option("tag", "Tag of the draft or message")]
                string tag,

                [Option("message", "The content of the message")]
                string content,

                [Option("mutex", "Will the roles be mutually exclusive")]
                Boolean rawMutex
            ) {
                if(!IsAuthorized(ctx.Member)) return;

                try {
                    await ctx.DeferResponseAsync();
                    bool mutex = (bool)rawMutex.ToBool();

                    EReactionMessage reactionMessage = new() { Content = content, Tag = tag, Mutex = mutex };
                    Ictx.ReactionMessages.Add(reactionMessage);
                    Ictx.SaveChanges();

                    await ctx.EditResponseAsync($"Added new {(mutex ? "mutex " : "")}draft with tag \"{tag}\" and content:\n{content}");

                    await ctx.FollowUpAsync("[Preview]\n" + CreateMessageBody(ctx, reactionMessage));

                } catch(Exception e) { await LogToSource(ctx, e); }
            }


            [SlashCommand("edit", "Edit a draft or message")]
            private async Task Edit(
                InteractionContext ctx,

                [Option("tag", "Tag of the draft or message")]
                string tag,

                [Option("message", "New message content")]
                string content = null,

                [Option("mutex", "New mutual exclusivity value")]
                Boolean rawMutex = (Boolean)2
            ) {
                if(!IsAuthorized(ctx.Member)) return;

                try {
                    await ctx.DeferResponseAsync();
                    bool? mutex = rawMutex.ToBool();

                    EReactionMessage reactionMessage =
                        Ictx.ReactionMessages
                        .FirstOrDefault(x => x.Tag == tag);

                    if(reactionMessage is null) {
                        await ctx.EditResponseAsync("Tag not found.");
                        return;
                    }

                    if(content is null && mutex is null) {
                        await ctx.EditResponseAsync("Updated nothing.");
                        return;
                    }

                    if(content is not null)
                        reactionMessage.Content = content;
                    if(mutex is null)
                        reactionMessage.Mutex = (bool)mutex;
                    Ictx.SaveChanges();


                    await ctx.EditResponseAsync(
                        $"Updated " +
                        $"{(content is not null ? "content" : "")}" +
                        $"{(content is not null && mutex is not null ? " and " : "")}" +
                        $"{(mutex is not null ? "mutex value" : "")}" +
                        $" for {DraftOrMessage(reactionMessage)}" +
                        $" tagged \"{tag}\"."
                    );

                    await ctx.FollowUpAsync("[Preview]\n" + CreateMessageBody(ctx, reactionMessage));

                } catch(Exception e) { await LogToSource(ctx, e); }
            }


            [SlashCommand("remove", "Remove a reaction role from a draft or message")]
            private async Task Remove(
                InteractionContext ctx,

                [Option("tag", "Tag of the draft or message")]
                string tag,

                [Option("role", "Role to remove from the draft or message")]
                DiscordRole role
            ) {
                if(!IsAuthorized(ctx.Member)) return;

                try {
                    await ctx.DeferResponseAsync();

                    EReactionMessage reactionMessage =
                        Ictx.ReactionMessages
                        .Include(x => x.Pairs)
                        .FirstOrDefault(x => x.Tag == tag);

                    if(reactionMessage is null) {
                        await ctx.EditResponseAsync("Tag not found.");
                        return;
                    }

                    EmojiRolePair pair = reactionMessage.Pairs.FirstOrDefault(x => x.RoleId == role.Id);

                    if(pair is null) {
                        await ctx.EditResponseAsync($"Role not present in {DraftOrMessage(reactionMessage)}.");
                        return;
                    }

                    reactionMessage.Pairs.Remove(pair);
                    Ictx.SaveChanges();
                    await UpdateReactionMessage(ctx, reactionMessage);

                    await ctx.EditResponseAsync($"Removed role from {DraftOrMessage(reactionMessage)}.");

                    await ctx.FollowUpAsync("[Preview]\n" + CreateMessageBody(ctx, reactionMessage));

                } catch(Exception e) { await LogToSource(ctx, e); }
            }


            private static string DraftOrMessage(EReactionMessage reactionMessage)
                => reactionMessage.MessageId is not null ? "message" : "draft";


            [SlashCommand("delete", "Delete a draft or message")]
            private async Task Delete(
                InteractionContext ctx,

                [Option("tag", "Tag of the draft or message")]
                string tag
            ) {
                if(!IsAuthorized(ctx.Member)) return;

                try {
                    await ctx.DeferResponseAsync();

                    EReactionMessage reactionMessage =
                        Ictx.ReactionMessages
                        .Include(x => x.Pairs)
                        .FirstOrDefault(x => x.Tag == tag);

                    if(reactionMessage is null) {
                        await ctx.EditResponseAsync("Tag not found.");
                        return;
                    } //TODO error handling

                    Ictx.ReactionMessages.Remove(reactionMessage);
                    Ictx.SaveChanges();

                    await ctx.EditResponseAsync($"Deleted {DraftOrMessage(reactionMessage)} tagged \"{tag}\"");

                    await ctx.FollowUpAsync($"[Deleted {DraftOrMessage(reactionMessage)}]\n" + CreateMessageBody(ctx, reactionMessage));

                } catch(Exception e) { await LogToSource(ctx, e); }
            }


            [SlashCommand("publish", "Publish a draft")]
            private async Task Publish(
                InteractionContext ctx,

                [Option("tag", "Tag of the draft")]
                string tag,

                [Option("channel", "Where the message will live")]
                DiscordChannel channel
            ) {
                if(!IsAuthorized(ctx.Member)) return;

                try {
                    await ctx.DeferResponseAsync();
                    
                    EReactionMessage reactionMessage =
                         Ictx.ReactionMessages
                         .Include(x => x.Pairs)
                         .FirstOrDefault(x => x.Tag == tag);

                    if(reactionMessage is null) {
                        await ctx.EditResponseAsync("Tag not found.");
                        return;
                    }

                    if(channel.IsCategory) {
                        await ctx.EditResponseAsync("Channel expected, got category.");
                        return;
                    }

                    reactionMessage.ChannelId = channel.Id;
                    DiscordMessage message = await channel.SendMessageAsync("Setting up reaction roles...");
                    reactionMessage.MessageId = message.Id;
                    await UpdateReactionMessage(ctx, reactionMessage);

                    await ctx.EditResponseAsync($"Published message tagged \"{tag}\" in channel {channel.Mention}:\n{message.JumpLink}");

                } catch(Exception e) { await LogToSource(ctx, e); }
            }


            [SlashCommand("list", "Lists all drafts and messages")]
            private async Task List(
                InteractionContext ctx
            ) {
                if(!IsAuthorized(ctx.Member)) return;

                try {
                    await ctx.DeferResponseAsync();

                    if(Ictx.ReactionMessages.Count() == 0) {
                        await ctx.EditResponseAsync("No messages or drafts found.");
                        return;
                    }

                    string content = "Found:";
                    bool flag;
                    foreach(EReactionMessage reactionMessage in Ictx.ReactionMessages) {
                        content +=
                            $"\n{DraftOrMessage(reactionMessage).FirstCharToUpper()}: " +
                            $"\"{reactionMessage.Tag}\"" +
                            @$"{(
                                (flag = reactionMessage.ChannelId is not null)
                                ? $" in {(await Illuminate.Client.GetChannelAsync((ulong)reactionMessage.ChannelId)).Mention}"
                                : ""
                            )}";
                    }

                    await ctx.EditResponseAsync(content);

                } catch(Exception e) { await LogToSource(ctx, e); }
            }


            [SlashCommand("preview", "Previews a draft, links to a message")]
            private async Task Preview(
                InteractionContext ctx,

                [Option("tag", "Tag of the draft")]
                string tag
            ) {
                if(!IsAuthorized(ctx.Member)) return;

                try {
                    await ctx.DeferResponseAsync();

                    EReactionMessage reactionMessage =
                         Ictx.ReactionMessages
                         .Include(x => x.Pairs)
                         .FirstOrDefault(x => x.Tag == tag);

                    if(reactionMessage is null) {
                        await ctx.EditResponseAsync("Tag not found.");
                        return;
                    }

                    if(reactionMessage.MessageId is not null) {
                        DiscordMessage message =
                            await (
                                await Illuminate.Client
                                .GetChannelAsync((ulong)reactionMessage.ChannelId)
                            )
                            .GetMessageAsync((ulong)reactionMessage.MessageId);
                        await ctx.EditResponseAsync($"[Link]\n{message.JumpLink}");
                    }

                    await ctx.EditResponseAsync("[Preview]\n" + CreateMessageBody(ctx, reactionMessage));

                } catch(Exception e) { await LogToSource(ctx, e); }
            }
        }


        internal enum Boolean {
            @true = 1,
            @false = 0
        }
    }
}