using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RaceManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRaceBankInfoAndRegistrationFee : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Fee",
                table: "Registrations",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "BankAccountHolder",
                table: "Races",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BankAccountNo",
                table: "Races",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BankName",
                table: "Races",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Fee",
                table: "Registrations");

            migrationBuilder.DropColumn(
                name: "BankAccountHolder",
                table: "Races");

            migrationBuilder.DropColumn(
                name: "BankAccountNo",
                table: "Races");

            migrationBuilder.DropColumn(
                name: "BankName",
                table: "Races");
        }
    }
}
