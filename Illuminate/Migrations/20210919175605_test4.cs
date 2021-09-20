using Microsoft.EntityFrameworkCore.Migrations;

namespace Illuminate.Migrations
{
    public partial class test4 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmojiId",
                table: "EmojiRolePair");

            migrationBuilder.AddColumn<string>(
                name: "EmojiName",
                table: "EmojiRolePair",
                type: "TEXT",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmojiName",
                table: "EmojiRolePair");

            migrationBuilder.AddColumn<ulong>(
                name: "EmojiId",
                table: "EmojiRolePair",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0ul);
        }
    }
}
