namespace Services {
    using DisCatSharp.EventArgs;
    using DisCatSharp;
    using System.Threading.Tasks;

    using Microsoft.EntityFrameworkCore.Internal;
    using System;
    using DisCatSharp.Entities;
    using System.Linq;
    using Illuminate;

#if DEBUG
    using static Illuminate.Debug;
    using DataLayer;
    using Commands;
#endif

    internal static class Services {
        internal static void RegisterServices(DiscordClient client) {
            client.GuildAvailable += SetUpForEcolinguist;

#if LOCAL
            client.MessageCreated += CheckMessage;
#else
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

            IlluminateContext ictx = new();

            string word = ictx.SwearWords.FirstOrDefault((word) => e.Message.Content.ToLowerInvariant().Contains(word.String))?.String;

            if(word is not null)
                await modChannel.SendMessageAsync($"Potential Swear Word Found:\nWord: ||{word}||\nChannel: {e.Channel.Mention}\n{e.Message.JumpLink}");
        }


        #region Auto Member Role


        private static async Task GuildMemberAddedAsync(DiscordClient sender, GuildMemberAddEventArgs e) {
            if(!ready) return;

            await e.Member.GrantRoleAsync(memberRole);
        }


#endregion
    }
}