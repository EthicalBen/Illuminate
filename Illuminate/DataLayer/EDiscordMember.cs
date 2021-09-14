namespace DataLayer {
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using DisCatSharp.Entities;

    using Microsoft.EntityFrameworkCore;

    internal class EDiscordMember{

        [Key]
        public int DbId { get; private set; }

        public ulong Id { get; private set; }

        public DateTime JoinedAt { get; private set; }

        public static explicit operator EDiscordMember(DiscordMember discordMember) {
            EDiscordMember member = new EDiscordMember();
            member.Id = discordMember.Id;

            return member;
        }
    }
}