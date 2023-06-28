namespace DataLayer {
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    using DisCatSharp.Entities;

    internal class EDiscordMember{

        [Key]
        public int DbId { get; private set; }

        public ulong Id { get; private set; }

        public List<ERole> LanguageRoles { get; private set; }

        public DateTimeOffset JoinedAt { get; private set; }

        public EDiscordGuild Guild { get; private set; }

        public List<EDiscordMessage> Messages { get; private set; }

        public static explicit operator EDiscordMember(DiscordMember discordMember) {
            EDiscordMember member = new EDiscordMember();
            member.Id = discordMember.Id;
            member.JoinedAt = DateTimeOffset.UtcNow;
            member.Messages = new();
            member.LanguageRoles = new();

            return member;
        }
    }
}