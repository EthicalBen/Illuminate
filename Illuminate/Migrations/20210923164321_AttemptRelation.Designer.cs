﻿// <auto-generated />
using System;
using DataLayer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Illuminate.Migrations
{
    [DbContext(typeof(IlluminateContext))]
    [Migration("20210923164321_AttemptRelation")]
    partial class AttemptRelation
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "5.0.9");

            modelBuilder.Entity("DataLayer.EDiscordGuild", b =>
                {
                    b.Property<int>("DbId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("Id")
                        .HasColumnType("INTEGER");

                    b.Property<ulong?>("memberRoleId")
                        .HasColumnType("INTEGER");

                    b.Property<ulong?>("modChannelId")
                        .HasColumnType("INTEGER");

                    b.HasKey("DbId");

                    b.ToTable("Guilds");
                });

            modelBuilder.Entity("DataLayer.EDiscordMember", b =>
                {
                    b.Property<int>("DbId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int?>("GuildDbId")
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("Id")
                        .HasColumnType("INTEGER");

                    b.Property<DateTimeOffset>("JoinedAt")
                        .HasColumnType("TEXT");

                    b.HasKey("DbId");

                    b.HasIndex("GuildDbId");

                    b.ToTable("Members");
                });

            modelBuilder.Entity("DataLayer.EDiscordMessage", b =>
                {
                    b.Property<int>("DbId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("TEXT");

                    b.Property<int?>("EDiscordMemberDbId")
                        .HasColumnType("INTEGER");

                    b.HasKey("DbId");

                    b.HasIndex("EDiscordMemberDbId");

                    b.ToTable("EDiscordMessage");
                });

            modelBuilder.Entity("DataLayer.EEmojiRolePair", b =>
                {
                    b.Property<int>("DbId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int?>("EReactionMessageDbId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("EmojiName")
                        .HasColumnType("TEXT");

                    b.Property<ulong>("RoleId")
                        .HasColumnType("INTEGER");

                    b.HasKey("DbId");

                    b.HasIndex("EReactionMessageDbId");

                    b.ToTable("EEmojiRolePair");
                });

            modelBuilder.Entity("DataLayer.EReactionMessage", b =>
                {
                    b.Property<int>("DbId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<ulong?>("ChannelId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Content")
                        .HasColumnType("TEXT");

                    b.Property<ulong?>("MessageId")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("Mutex")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Tag")
                        .HasColumnType("TEXT");

                    b.HasKey("DbId");

                    b.ToTable("ReactionMessages");
                });

            modelBuilder.Entity("DataLayer.ERole", b =>
                {
                    b.Property<int>("DbId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int?>("EDiscordMemberDbId")
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("RoleId")
                        .HasColumnType("INTEGER");

                    b.HasKey("DbId");

                    b.HasIndex("EDiscordMemberDbId");

                    b.ToTable("ERole");
                });

            modelBuilder.Entity("DataLayer.EString", b =>
                {
                    b.Property<int>("DbId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("String")
                        .HasColumnType("TEXT");

                    b.HasKey("DbId");

                    b.ToTable("SwearWords");
                });

            modelBuilder.Entity("DataLayer.EDiscordMember", b =>
                {
                    b.HasOne("DataLayer.EDiscordGuild", "Guild")
                        .WithMany("Members")
                        .HasForeignKey("GuildDbId");

                    b.Navigation("Guild");
                });

            modelBuilder.Entity("DataLayer.EDiscordMessage", b =>
                {
                    b.HasOne("DataLayer.EDiscordMember", null)
                        .WithMany("Messages")
                        .HasForeignKey("EDiscordMemberDbId");
                });

            modelBuilder.Entity("DataLayer.EEmojiRolePair", b =>
                {
                    b.HasOne("DataLayer.EReactionMessage", null)
                        .WithMany("Pairs")
                        .HasForeignKey("EReactionMessageDbId");
                });

            modelBuilder.Entity("DataLayer.ERole", b =>
                {
                    b.HasOne("DataLayer.EDiscordMember", null)
                        .WithMany("LanguageRoles")
                        .HasForeignKey("EDiscordMemberDbId");
                });

            modelBuilder.Entity("DataLayer.EDiscordGuild", b =>
                {
                    b.Navigation("Members");
                });

            modelBuilder.Entity("DataLayer.EDiscordMember", b =>
                {
                    b.Navigation("LanguageRoles");

                    b.Navigation("Messages");
                });

            modelBuilder.Entity("DataLayer.EReactionMessage", b =>
                {
                    b.Navigation("Pairs");
                });
#pragma warning restore 612, 618
        }
    }
}
