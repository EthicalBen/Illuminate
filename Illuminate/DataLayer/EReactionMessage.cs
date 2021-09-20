namespace DataLayer {
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    using DisCatSharp.CommandsNext.Converters;
    using DisCatSharp.Entities;

    using Illuminate;

    using Microsoft.EntityFrameworkCore;

    internal class EReactionMessage {

        [Key]
        public int DbId { get; private set; }

        public string Tag { get; set; }

        public string Content { get; set; }

        public bool Mutex { get; set; }

        public ulong? MessageId { get; set; }

        public ulong? ChannelId { get; set; }

        public List<EmojiRolePair> Pairs { get; set; } //Dictionary?

        public EReactionMessage() {
            Pairs = new();
        }

    }

    internal class EmojiRolePair {

        [Key]
        public int DbId { get; private set; }

        public string EmojiName { get; set; }

        public ulong RoleId { get; set; }
    }
}