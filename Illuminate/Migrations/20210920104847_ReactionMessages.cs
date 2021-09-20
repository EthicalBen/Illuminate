using Microsoft.EntityFrameworkCore.Migrations;

namespace Illuminate.Migrations
{
    public partial class ReactionMessages : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Id",
                table: "ReactionMessages",
                newName: "Mutex");

            migrationBuilder.AddColumn<ulong>(
                name: "ChannelId",
                table: "ReactionMessages",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Content",
                table: "ReactionMessages",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<ulong>(
                name: "MessageId",
                table: "ReactionMessages",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Tag",
                table: "ReactionMessages",
                type: "TEXT",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChannelId",
                table: "ReactionMessages");

            migrationBuilder.DropColumn(
                name: "Content",
                table: "ReactionMessages");

            migrationBuilder.DropColumn(
                name: "MessageId",
                table: "ReactionMessages");

            migrationBuilder.DropColumn(
                name: "Tag",
                table: "ReactionMessages");

            migrationBuilder.RenameColumn(
                name: "Mutex",
                table: "ReactionMessages",
                newName: "Id");
        }
    }
}
