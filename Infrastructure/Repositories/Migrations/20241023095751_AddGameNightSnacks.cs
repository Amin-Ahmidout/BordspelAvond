using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BordspelAvond.Migrations
{
    /// <inheritdoc />
    public partial class AddGameNightSnacks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GameNightSnack",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GameNightId = table.Column<int>(type: "int", nullable: false),
                    SnackName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsVegetarian = table.Column<bool>(type: "bit", nullable: false),
                    IsLactoseFree = table.Column<bool>(type: "bit", nullable: false),
                    IsNutFree = table.Column<bool>(type: "bit", nullable: false),
                    IsAlcoholFree = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameNightSnack", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GameNightSnack_GameNights_GameNightId",
                        column: x => x.GameNightId,
                        principalTable: "GameNights",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GameNightSnack_GameNightId",
                table: "GameNightSnack",
                column: "GameNightId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GameNightSnack");
        }
    }
}
