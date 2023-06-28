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
    using DisCatSharp.ApplicationCommands;

    using Illuminate;

    using static CommandUtilities;
    using Microsoft.EntityFrameworkCore;
    using Illuminate.Migrations;
    using Microsoft.CSharp.RuntimeBinder;

    internal class SlashCommands:ApplicationCommandsModule {

        [SlashCommandGroup("swear-words", "The swear words module", true)]
        private class SwearWords {
            [SlashCommand("add", "Add a swear word")]
            private async Task Add(InteractionContext ctx, [Option("word", "The word to add")] string word) {
#if RELEASE
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
#if RELEASE
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
#if RELEASE
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
#if RELEASE
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
#if RELEASE
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
#if RELEASE
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
#if RELEASE
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
#if RELEASE
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

                    EEmojiRolePair existingPair = reactionMessage.Pairs.FirstOrDefault(p => p.RoleId == role.Id);
                    if(existingPair is not null) {
                        if(existingPair.EmojiName == emoji.GetDiscordName()) {
                            await ctx.EditResponseAsync("Nothing to add.");
                            return;
                        }

                        existingPair.EmojiName = emoji.GetDiscordName();
                        Ictx.SaveChanges();
                        await UpdateReactionMessage(ctx, reactionMessage);

                        await ctx.EditResponseAsync($"Updated Emoji for existing role.");

                        await ctx.FollowUpAsync("[Preview]\n" + CreateMessageBody(ctx, reactionMessage));
                        return;
                    }

                    reactionMessage.Pairs.Add(new() { EmojiName = emoji.GetDiscordName(), RoleId = role.Id });
                    Ictx.SaveChanges();//reactionMessage.Pairs = reactionMessage.Pairs; // Really tho?
                    await UpdateReactionMessage(ctx, reactionMessage);

                    await ctx.EditResponseAsync($"Added {role.Mention} with reaction {emoji} to \"{tag}\"");

                    await ctx.FollowUpAsync("[Preview]\n" + CreateMessageBody(ctx, reactionMessage));
                } catch(Exception e) { await LogToSource(ctx, e); }
#endif
            }


#if RELEASE
            private static string DraftOrMessage(EReactionMessage reactionMessage)
                => reactionMessage.MessageId is not null ? "message" : "draft";


            private static async Task UpdateReactionMessage(InteractionContext ctx, EReactionMessage reactionMessage, DiscordMessage message = null) {
                if(reactionMessage.MessageId is not null) {
                    if(message is null)
                        message =
                            await Illuminate.Client
                            .GetChannelAsync((ulong)reactionMessage.ChannelId)
                            .ContinueWith(t => t.Result.GetMessageAsync((ulong)reactionMessage.MessageId).Result);

                    await message.DeleteAllReactionsAsync();

                    foreach(EEmojiRolePair pair in reactionMessage.Pairs)
                        await message.CreateReactionAsync(DiscordEmoji.FromName(Illuminate.Client, pair.EmojiName));

                    string content = CreateMessageBody(ctx, reactionMessage);
                    await message.ModifyAsync(content);
                }
            }


            private static string CreateMessageBody(InteractionContext ctx, EReactionMessage reactionMessage) {
                string content = reactionMessage.Content;
                foreach(EEmojiRolePair pair in reactionMessage.Pairs) {
                    content += $"\n{pair.EmojiName}: {ctx.Channel.Guild.GetRole(pair.RoleId).Mention}";
                }

                return content;
            }
#endif


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
#if RELEASE
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
#endif
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
#if RELEASE
                if(!IsAuthorized(ctx.Member)) return;

                try {
                    await ctx.DeferResponseAsync();
                    bool? mutex = rawMutex.ToBool();

                    EReactionMessage reactionMessage =
                        Ictx.ReactionMessages
                        .Include(x => x.Pairs)
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
                    if(mutex is not null)
                        reactionMessage.Mutex = (bool)mutex;
                    Ictx.Entry(reactionMessage).State = EntityState.Modified;
                    Ictx.SaveChanges();


                    await ctx.EditResponseAsync(
                        $"Updated " +
                        $"{(content is not null ? "content" : "")}" +
                        $"{(content is not null && mutex is not null ? " and " : "")}" +
                        $"{(mutex is not null ? "mutex value" : "")}" +
                        $" for {DraftOrMessage(reactionMessage)}" +
                        $" tagged \"{tag}\"."
                    );

                    if(reactionMessage.MessageId is not null && content is not null) {
                        DiscordMessage message =
                            await (
                                await Illuminate.Client
                                .GetChannelAsync((ulong)reactionMessage.ChannelId)
                            )
                            .GetMessageAsync((ulong)reactionMessage.MessageId);
                        await UpdateReactionMessage(ctx, reactionMessage, message);
                        await ctx.FollowUpAsync($"[Link]\n{message.JumpLink}");
                    } else {
                        await ctx.FollowUpAsync("[Preview]\n" + CreateMessageBody(ctx, reactionMessage));
                    }

                } catch(Exception e) { await LogToSource(ctx, e); }
#endif
            }


            [SlashCommand("remove", "Remove a reaction role from a draft or message")]
            private async Task Remove(
                InteractionContext ctx,

                [Option("tag", "Tag of the draft or message")]
                string tag,

                [Option("role", "Role to remove from the draft or message")]
                DiscordRole role
            ) {
#if RELEASE
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

                    EEmojiRolePair pair = reactionMessage.Pairs.FirstOrDefault(x => x.RoleId == role.Id);

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
#endif
            }


            [SlashCommand("delete", "Delete a draft or message")]
            private async Task Delete(
                InteractionContext ctx,

                [Option("tag", "Tag of the draft or message")]
                string tag
            ) {
#if RELEASE
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

                    if(reactionMessage.MessageId is not null) {
                        try {
                            DiscordChannel channel = await Illuminate.Client.GetChannelAsync((ulong)reactionMessage.ChannelId);
                            DiscordMessage message = await channel.GetMessageAsync((ulong)reactionMessage.MessageId);
                            channel.DeleteMessageAsync(message);
                        } catch { }
                    }


                    await ctx.EditResponseAsync($"Deleted {DraftOrMessage(reactionMessage)} tagged \"{tag}\"");

                    await ctx.FollowUpAsync($"[Deleted {DraftOrMessage(reactionMessage)}]\n" + CreateMessageBody(ctx, reactionMessage));

                } catch(Exception e) { await LogToSource(ctx, e); }
#endif
            }


            [SlashCommand("publish", "Publish a draft")]
            private async Task Publish(
                InteractionContext ctx,

                [Option("tag", "Tag of the draft")]
                string tag,

                [Option("channel", "Where the message will live")]
                DiscordChannel channel
            ) {
#if RELEASE
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
                    Ictx.Entry(reactionMessage).State = EntityState.Modified;
                    Ictx.SaveChanges();
                    await UpdateReactionMessage(ctx, reactionMessage, message);

                    await ctx.EditResponseAsync($"Published message tagged \"{tag}\" in channel {channel.Mention}:\n{message.JumpLink}");

                } catch(Exception e) { await LogToSource(ctx, e); }
#endif
            }


            [SlashCommand("list", "Lists all drafts and messages")]
            private async Task List(
                InteractionContext ctx
            ) {
#if RELEASE
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
#endif
            }


            [SlashCommand("preview", "Previews a draft, links to a message")]
            private async Task Preview(
                InteractionContext ctx,

                [Option("tag", "Tag of the draft")]
                string tag
            ) {
#if RELEASE
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
#endif
            }
        }


        [SlashCommandGroup("raid-protect", "The raid protection module")]
        private class RaidProtect {

            [SlashCommand("state", "Enables or disables the module")]
            private async Task State(
                InteractionContext ctx,

                [Option("state", "On? Off?")]
                State state
            ) {
                if(!IsAuthorized(ctx.Member)) return;

                try {
                    await ctx.DeferResponseAsync();

                    await ctx.EditResponseAsync("[Not Implemented]");

                } catch(Exception e) { await LogToSource(ctx, e); }
            }
        }


        [SlashCommand("debug", "What effect does this command have? Roll a D20 to find out!")]
        private async Task Debug(
            InteractionContext ctx
        ) {
#if DEBUG
            if(ctx.User.Id != 290921830515212298) { //If not me: Play dead.
                await ctx.CreateResponseAsync($"You rolled a {new Random().Next(1, 20)}, but nothing happened. Try again?");
                return;
            }

            DiscordChannel devChannel = Services.Services.Ecolinguist.GetChannel(875797784354226186);

            try {
                DiscordChannelWrapper channel = ctx.Channel;
                channel ??= ctx.Guild.GetThread(ctx.Interaction.ChannelId);

                await ctx.CreateResponseAsync("Sending message...");
                await channel.SendMessageAsync("test");
            } catch(Exception e) {
                await devChannel.SendMessageAsync(e.Message);
            }
#endif
        }

        class DiscordChannelWrapper {
            static DiscordChannelWrapper() {
                if(
                    false
                    // || Test 1
                    // || Test 2
                    // || Test 3
                    // || Test 4






                ) throw new RuntimeBinderException();
            }


            private DiscordChannelWrapper() { }

            dynamic channel;
            public Task<DiscordMessage> SendMessageAsync(string content) =>
                channel.SendMessageAsync(content);

            public static implicit operator DiscordChannelWrapper(DiscordChannel channel) {
                if(channel is null) return null;

                DiscordChannelWrapper channelWrapper = new();
                channelWrapper.channel = channel;
                return channelWrapper;
            }

            public static implicit operator DiscordChannelWrapper(DiscordThreadChannel channel) {
                if(channel is null) return null;

                DiscordChannelWrapper channelWrapper = new();
                channelWrapper.channel = channel;
                return channelWrapper;
            }
        }



        internal enum Boolean {
            @true = 1,
            @false = 0
        }


        internal enum State {
            enable = 1,
            disable = 0
        }
    }
}