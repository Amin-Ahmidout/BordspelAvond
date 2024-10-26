using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BordspelAvond.Migrations
{
    /// <inheritdoc />
    public partial class AddGameNightSnacks2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GameNightSnack_GameNights_GameNightId",
                table: "GameNightSnack");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GameNightSnack",
                table: "GameNightSnack");

            migrationBuilder.RenameTable(
                name: "GameNightSnack",
                newName: "GameNightSnacks");

            migrationBuilder.RenameIndex(
                name: "IX_GameNightSnack_GameNightId",
                table: "GameNightSnacks",
                newName: "IX_GameNightSnacks_GameNightId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_GameNightSnacks",
                table: "GameNightSnacks",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_GameNightSnacks_GameNights_GameNightId",
                table: "GameNightSnacks",
                column: "GameNightId",
                principalTable: "GameNights",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GameNightSnacks_GameNights_GameNightId",
                table: "GameNightSnacks");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GameNightSnacks",
                table: "GameNightSnacks");

            migrationBuilder.RenameTable(
                name: "GameNightSnacks",
                newName: "GameNightSnack");

            migrationBuilder.RenameIndex(
                name: "IX_GameNightSnacks_GameNightId",
                table: "GameNightSnack",
                newName: "IX_GameNightSnack_GameNightId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_GameNightSnack",
                table: "GameNightSnack",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_GameNightSnack_GameNights_GameNightId",
                table: "GameNightSnack",
                column: "GameNightId",
                principalTable: "GameNights",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
