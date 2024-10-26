using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BordspelAvond.Migrations
{
    /// <inheritdoc />
    public partial class AddDietaryOptions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ProvidesAlcoholFreeOptions",
                table: "GameNights",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ProvidesLactoseFreeOptions",
                table: "GameNights",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ProvidesNutsFreeOptions",
                table: "GameNights",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ProvidesVegetarianOptions",
                table: "GameNights",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProvidesAlcoholFreeOptions",
                table: "GameNights");

            migrationBuilder.DropColumn(
                name: "ProvidesLactoseFreeOptions",
                table: "GameNights");

            migrationBuilder.DropColumn(
                name: "ProvidesNutsFreeOptions",
                table: "GameNights");

            migrationBuilder.DropColumn(
                name: "ProvidesVegetarianOptions",
                table: "GameNights");
        }
    }
}
