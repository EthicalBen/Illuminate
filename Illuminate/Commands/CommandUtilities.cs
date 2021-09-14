namespace Commands {
    using System;
    using System.Linq;
    using System.Reflection.Metadata;
    using System.Threading.Tasks;
    using DisCatSharp.CommandsNext;
    using DisCatSharp.Entities;
    using DisCatSharp.SlashCommands;

    using static Services.Services;

    internal static class CommandUtilities {
        internal static async Task<bool> IsAuthorized(DiscordMember member) {
            return
                member is not null
                && (
                    member == Ecolinguist.Owner
                    || (
                        ModeratorRole is not null
                        && member.Roles.Contains(ModeratorRole)
                    )
                    || member.Roles.Any(role => role.Permissions.HasFlag(DisCatSharp.Permissions.Administrator))
                );
        }


        internal static void LogToSource(CommandContext ctx, Exception e)
            => ctx.Channel.SendMessageAsync('>' + e.Message/* + "\n" + e.StackTrace.Replace("`", @"\`")*/);
        internal static void LogToSource(InteractionContext ctx, Exception e)
            => ctx.Channel.SendMessageAsync('>' + e.Message/* + "\n" + e.StackTrace.Replace("`", @"\`")*/);
    }
}