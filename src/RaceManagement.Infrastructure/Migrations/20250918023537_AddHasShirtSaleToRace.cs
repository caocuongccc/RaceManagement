using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RaceManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddHasShirtSaleToRace : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HasShirtSale",
                table: "Races",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HasShirtSale",
                table: "Races");
        }
    }
}
