using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Illuminate.Migrations
{
    public partial class Guilds1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmojiRolePair");

            migrationBuilder.AddColumn<int>(
                name: "EDiscordGuildDbId",
                table: "Members",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "EDiscordMessage",
                columns: table => new
                {
                    DbId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Id = table.Column<ulong>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EDiscordMemberDbId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EDiscordMessage", x => x.DbId);
                    table.ForeignKey(
                        name: "FK_EDiscordMessage_Members_EDiscordMemberDbId",
                        column: x => x.EDiscordMemberDbId,
                        principalTable: "Members",
                        principalColumn: "DbId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EEmojiRolePair",
                columns: table => new
                {
                    DbId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EmojiName = table.Column<string>(type: "TEXT", nullable: true),
                    RoleId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    EReactionMessageDbId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EEmojiRolePair", x => x.DbId);
                    table.ForeignKey(
                        name: "FK_EEmojiRolePair_ReactionMessages_EReactionMessageDbId",
                        column: x => x.EReactionMessageDbId,
                        principalTable: "ReactionMessages",
                        principalColumn: "DbId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ERole",
                columns: table => new
                {
                    DbId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RoleId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    EDiscordMemberDbId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ERole", x => x.DbId);
                    table.ForeignKey(
                        name: "FK_ERole_Members_EDiscordMemberDbId",
                        column: x => x.EDiscordMemberDbId,
                        principalTable: "Members",
                        principalColumn: "DbId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Guilds",
                columns: table => new
                {
                    DbId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Id = table.Column<ulong>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Guilds", x => x.DbId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Members_EDiscordGuildDbId",
                table: "Members",
                column: "EDiscordGuildDbId");

            migrationBuilder.CreateIndex(
                name: "IX_EDiscordMessage_EDiscordMemberDbId",
                table: "EDiscordMessage",
                column: "EDiscordMemberDbId");

            migrationBuilder.CreateIndex(
                name: "IX_EEmojiRolePair_EReactionMessageDbId",
                table: "EEmojiRolePair",
                column: "EReactionMessageDbId");

            migrationBuilder.CreateIndex(
                name: "IX_ERole_EDiscordMemberDbId",
                table: "ERole",
                column: "EDiscordMemberDbId");

            migrationBuilder.AddForeignKey(
                name: "FK_Members_Guilds_EDiscordGuildDbId",
                table: "Members",
                column: "EDiscordGuildDbId",
                principalTable: "Guilds",
                principalColumn: "DbId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Members_Guilds_EDiscordGuildDbId",
                table: "Members");

            migrationBuilder.DropTable(
                name: "EDiscordMessage");

            migrationBuilder.DropTable(
                name: "EEmojiRolePair");

            migrationBuilder.DropTable(
                name: "ERole");

            migrationBuilder.DropTable(
                name: "Guilds");

            migrationBuilder.DropIndex(
                name: "IX_Members_EDiscordGuildDbId",
                table: "Members");

            migrationBuilder.DropColumn(
                name: "EDiscordGuildDbId",
                table: "Members");

            migrationBuilder.CreateTable(
                name: "EmojiRolePair",
                columns: table => new
                {
                    DbId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EReactionMessageDbId = table.Column<int>(type: "INTEGER", nullable: true),
                    EmojiName = table.Column<string>(type: "TEXT", nullable: true),
                    RoleId = table.Column<ulong>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmojiRolePair", x => x.DbId);
                    table.ForeignKey(
                        name: "FK_EmojiRolePair_ReactionMessages_EReactionMessageDbId",
                        column: x => x.EReactionMessageDbId,
                        principalTable: "ReactionMessages",
                        principalColumn: "DbId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmojiRolePair_EReactionMessageDbId",
                table: "EmojiRolePair",
                column: "EReactionMessageDbId");
        }
    }
}
