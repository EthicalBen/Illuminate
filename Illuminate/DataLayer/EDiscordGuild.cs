namespace DataLayer {
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    using DisCatSharp.Entities;

    using Illuminate;

    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    internal class EDiscordGuild {


        [Key]
        public int DbId { get; private set; }


        public ulong Id { get; private set; }


        public List<EDiscordMember> Members { get; private set; }


        [NotMapped]
        public DiscordChannel ModChannel {
            get {
                return
                    modChannelId is null
                    ? null
                    : modChannel ??= Illuminate.Client.GetChannelAsync((ulong)modChannelId).Result;
            }
            set {
                modChannel = value;
                modChannelId = value.Id;
            }
        }
        private DiscordChannel modChannel;
        private ulong? modChannelId;


        [NotMapped]
        public DiscordRole MemberRole {
            get {
                return
                    memberRoleId is null
                    ? null
                    : memberRole ??= Illuminate.Client.GetGuildAsync(Id).Result.GetRole((ulong)memberRoleId);
            }
            set {
                memberRole = value;
                memberRoleId = value.Id;
            }
        }
        private DiscordRole memberRole;
        private ulong? memberRoleId;


        public EDiscordGuild() {
            Members = new();
        }


        internal class Configuration:IEntityTypeConfiguration<EDiscordGuild> {
            public void Configure(EntityTypeBuilder<EDiscordGuild> builder) {
                builder.Property(x => x.modChannelId);
                builder.Property(x => x.memberRoleId);
            }
        }
    }
}