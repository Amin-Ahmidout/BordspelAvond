using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BordspelAvond.Migrations.AuthDb
{
    /// <inheritdoc />
    public partial class AddDietaryOptionsToPlayer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AvoidsAlcohol",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasLactoseAllergy",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasNutsAllergy",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsVegetarian",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvoidsAlcohol",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "HasLactoseAllergy",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "HasNutsAllergy",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "IsVegetarian",
                table: "AspNetUsers");
        }
    }
}
