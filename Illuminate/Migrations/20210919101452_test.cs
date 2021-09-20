using Microsoft.EntityFrameworkCore.Migrations;

namespace Illuminate.Migrations
{
    public partial class test : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<ulong>(
                name: "GuildId",
                table: "ReactionMessages",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0ul);

            migrationBuilder.CreateTable(
                name: "EmojiRolePair",
                columns: table => new
                {
                    DbId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EmojiId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    RoleId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    EReactionMessageDbId = table.Column<int>(type: "INTEGER", nullable: true)
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

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmojiRolePair");

            migrationBuilder.DropColumn(
                name: "GuildId",
                table: "ReactionMessages");
        }
    }
}
