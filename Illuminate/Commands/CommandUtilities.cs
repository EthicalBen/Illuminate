namespace Commands {
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using DataLayer;

    using DisCatSharp;
    using DisCatSharp.CommandsNext;
    using DisCatSharp.Entities;
    using DisCatSharp.ApplicationCommands;

    using static Services.Services;

    internal static class CommandUtilities {

        [ThreadStatic]
        private static IlluminateContext ictx;
        internal static IlluminateContext Ictx {
            get => ictx ??= new();
        }

        internal static bool IsAuthorized(DiscordMember member)
            => member is not null
            && (
                member == Ecolinguist.Owner
                || (
                    ModeratorRole is not null
                    && member.Roles.Contains(ModeratorRole)
                )
                || member.Roles.Any(role => role.Permissions.HasFlag(DisCatSharp.Permissions.Administrator))
            );


        internal static async Task LogToSource(CommandContext ctx, Exception e)
            => await ctx.Channel.SendMessageAsync('>' + e.Message/* + "\n" + e.StackTrace.Replace("`", @"\`")*/);


        internal static async Task LogToSource(InteractionContext ctx, Exception e) {
            try {
                await ctx.CreateResponseAsync('>' + e.Message);
            } catch {
                await ctx.Channel.SendMessageAsync('>' + e.Message);
            }
        }


        internal static async Task CreateResponseAsync(this InteractionContext ctx, string content)
            => await ctx.Interaction.CreateResponseAsync(content);


        internal static async Task CreateResponseAsync(this DiscordInteraction interaction, string content) {
            await interaction.CreateResponseAsync(
                InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder()
                .WithContent(content)
                .AsEphemeral(true)
            );
        }
        

        internal static async Task<DiscordMessage> EditResponseAsync(this InteractionContext ctx, string content)
            => await ctx.EditResponseAsync(
                new DiscordWebhookBuilder()
                .WithContent(content)
            );


        internal static async Task DeferResponseAsync(this InteractionContext ctx)
            => await ctx.CreateResponseAsync(
                InteractionResponseType.DeferredChannelMessageWithSource,
                new DiscordInteractionResponseBuilder()
                .AsEphemeral(true)
            );


        internal static async Task<DiscordMessage> FollowUpAsync(this InteractionContext ctx, string content)
            => await ctx.FollowUpAsync(
                new DiscordFollowupMessageBuilder()
                .AsEphemeral(true)
                .WithContent(content)
            );


        internal static bool? ToBool(this SlashCommands.Boolean boolean)
            => bool.TryParse(boolean.ToString(), out bool result)
            ? result
            : null;
    }
}