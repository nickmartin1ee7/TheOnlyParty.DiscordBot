using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TheOnlyParty.DiscordBot.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserReports",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    TotalMessages = table.Column<int>(type: "INTEGER", nullable: false),
                    PositiveMessages = table.Column<int>(type: "INTEGER", nullable: false),
                    NegativeMessages = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserReports", x => x.UserId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserReports");
        }
    }
}
