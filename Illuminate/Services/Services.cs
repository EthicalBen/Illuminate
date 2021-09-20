namespace Services {
#if DEBUG
    using static Illuminate.Debug;
#endif
    using System.Linq;
    using System.Threading.Tasks;

    using DisCatSharp;
    using DisCatSharp.Entities;
    using DisCatSharp.EventArgs;

    using static Commands.CommandUtilities;
    using System;
    using System.Collections.Generic;
    using DataLayer;
    using Microsoft.EntityFrameworkCore;

    internal static class Services {
        internal static void RegisterServices(DiscordClient client) {
            client.GuildAvailable += SetUpForEcolinguist;

#if LOCAL
            client.MessageReactionAdded += CheckReactions;
#else
            client.MessageCreated += CheckMessage;
            client.GuildMemberAdded += GuildMemberAddedAsync;
#endif
        }


        internal static DiscordGuild Ecolinguist;
        private static DiscordRole memberRole;
        private static DiscordChannel modChannel;
        internal static DiscordRole ModeratorRole { get; private set; }

        private static bool ready;
        private static Task SetUpForEcolinguist(DiscordClient sender, GuildCreateEventArgs e) {
            if(e.Guild.Name == "Ecolinguist") {
                Ecolinguist = e.Guild;
                if(Ecolinguist is not null) {
                    memberRole = Ecolinguist.Roles.FirstOrDefault(item => item.Value.Name == "Member").Value;
                    ModeratorRole = Ecolinguist.Roles.FirstOrDefault(item => item.Value.Name == "Moderator").Value;
                    modChannel = Ecolinguist.Channels.FirstOrDefault(item => item.Value.Name == "moderators").Value;
                }
                ready = memberRole is not null && ModeratorRole is not null && modChannel is not null;
            }
            return Task.CompletedTask;
        }

        private static async Task CheckMessage(DiscordClient client, MessageCreateEventArgs e) {
            if(
                !ready
                || e.Author == client.CurrentUser
                || false//await CommandUtilities.IsAuthorized(await e.Guild.GetMemberAsync(e.Author.Id))
            ) return;

            string word = Ictx.SwearWords.FirstOrDefault((word) => e.Message.Content.ToLowerInvariant().Contains(word.String))?.String;

            if(word is not null)
                await modChannel.SendMessageAsync($"Potential Swear Word Found:\nWord: ||{word}||\nChannel: {e.Channel.Mention}\n{e.Message.JumpLink}");
        }

        private static async Task GuildMemberAddedAsync(DiscordClient sender, GuildMemberAddEventArgs e) {
            if(!ready) return;

            await e.Member.GrantRoleAsync(memberRole);
        }


        private static async Task CheckReactions(DiscordClient sender, MessageReactionAddEventArgs e) {
            EReactionMessage reactionMessage =
                Ictx.ReactionMessages
                .Include(r => r.Pairs)
                .FirstOrDefault(r => r.MessageId == e.Message.Id && r.ChannelId == e.Channel.Id);

            if(reactionMessage is not null) {
                if(e.User == sender.CurrentUser) return;
                if(e.User != sender.CurrentUser) await e.Message.DeleteReactionAsync(e.Emoji, e.User);

                ulong? roleId = reactionMessage.Pairs.FirstOrDefault(x => x.EmojiName == e.Emoji.GetDiscordName())?.RoleId;

                DiscordRole role = roleId is not null ? e.Guild.GetRole(roleId.Value) : null;

                if(role is not null) {
                    if(!((DiscordMember)e.User).Roles.Contains(role)) {
                        await ((DiscordMember)e.User).GrantRoleAsync(role);
                    } else {
                        await ((DiscordMember)e.User).RevokeRoleAsync(role);
                    }
                }
            }
        }
    }
}