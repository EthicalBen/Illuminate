using Microsoft.EntityFrameworkCore.Migrations;

namespace Illuminate.Migrations
{
    public partial class AttemptRelation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Members_Guilds_EDiscordGuildDbId",
                table: "Members");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "EDiscordMessage");

            migrationBuilder.RenameColumn(
                name: "EDiscordGuildDbId",
                table: "Members",
                newName: "GuildDbId");

            migrationBuilder.RenameIndex(
                name: "IX_Members_EDiscordGuildDbId",
                table: "Members",
                newName: "IX_Members_GuildDbId");

            migrationBuilder.AddColumn<ulong>(
                name: "memberRoleId",
                table: "Guilds",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<ulong>(
                name: "modChannelId",
                table: "Guilds",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Members_Guilds_GuildDbId",
                table: "Members",
                column: "GuildDbId",
                principalTable: "Guilds",
                principalColumn: "DbId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Members_Guilds_GuildDbId",
                table: "Members");

            migrationBuilder.DropColumn(
                name: "memberRoleId",
                table: "Guilds");

            migrationBuilder.DropColumn(
                name: "modChannelId",
                table: "Guilds");

            migrationBuilder.RenameColumn(
                name: "GuildDbId",
                table: "Members",
                newName: "EDiscordGuildDbId");

            migrationBuilder.RenameIndex(
                name: "IX_Members_GuildDbId",
                table: "Members",
                newName: "IX_Members_EDiscordGuildDbId");

            migrationBuilder.AddColumn<ulong>(
                name: "Id",
                table: "EDiscordMessage",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0ul);

            migrationBuilder.AddForeignKey(
                name: "FK_Members_Guilds_EDiscordGuildDbId",
                table: "Members",
                column: "EDiscordGuildDbId",
                principalTable: "Guilds",
                principalColumn: "DbId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
