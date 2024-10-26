using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BordspelAvond.Migrations
{
    /// <inheritdoc />
    public partial class AddGameNightGamesRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GameGameNight");

            migrationBuilder.AddColumn<int>(
                name: "GameId",
                table: "GameNights",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "GameNightGames",
                columns: table => new
                {
                    GameNightId = table.Column<int>(type: "int", nullable: false),
                    GameId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameNightGames", x => new { x.GameNightId, x.GameId });
                    table.ForeignKey(
                        name: "FK_GameNightGames_GameNights_GameNightId",
                        column: x => x.GameNightId,
                        principalTable: "GameNights",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GameNightGames_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GameNights_GameId",
                table: "GameNights",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "IX_GameNightGames_GameId",
                table: "GameNightGames",
                column: "GameId");

            migrationBuilder.AddForeignKey(
                name: "FK_GameNights_Games_GameId",
                table: "GameNights",
                column: "GameId",
                principalTable: "Games",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GameNights_Games_GameId",
                table: "GameNights");

            migrationBuilder.DropTable(
                name: "GameNightGames");

            migrationBuilder.DropIndex(
                name: "IX_GameNights_GameId",
                table: "GameNights");

            migrationBuilder.DropColumn(
                name: "GameId",
                table: "GameNights");

            migrationBuilder.CreateTable(
                name: "GameGameNight",
                columns: table => new
                {
                    GameId = table.Column<int>(type: "int", nullable: false),
                    GameNightId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameGameNight", x => new { x.GameId, x.GameNightId });
                    table.ForeignKey(
                        name: "FK_GameGameNight_GameNights_GameNightId",
                        column: x => x.GameNightId,
                        principalTable: "GameNights",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GameGameNight_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GameGameNight_GameNightId",
                table: "GameGameNight",
                column: "GameNightId");
        }
    }
}
