using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RaceManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EnhancedRaceShirtSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add new columns to Races table
            migrationBuilder.AddColumn<string>(
                name: "GoogleCredentialPath",
                table: "Races",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            // Add new columns to Registrations table
            migrationBuilder.AddColumn<string>(
                name: "BibName",
                table: "Registrations",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateOfBirth",
                table: "Registrations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RawBirthInput",
                table: "Registrations",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShirtCategory",
                table: "Registrations",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShirtType",
                table: "Registrations",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            // Create RaceShirtTypes table
            migrationBuilder.CreateTable(
                name: "RaceShirtTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RaceId = table.Column<int>(type: "int", nullable: false),
                    ShirtCategory = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ShirtType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    AvailableSizes = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RaceShirtTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RaceShirtTypes_Races_RaceId",
                        column: x => x.RaceId,
                        principalTable: "Races",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Create indexes
            migrationBuilder.CreateIndex(
                name: "IX_RaceShirtTypes_RaceId",
                table: "RaceShirtTypes",
                column: "RaceId");

            migrationBuilder.CreateIndex(
                name: "IX_RaceShirtTypes_RaceId_ShirtCategory_ShirtType",
                table: "RaceShirtTypes",
                columns: new[] { "RaceId", "ShirtCategory", "ShirtType" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop RaceShirtTypes table
            migrationBuilder.DropTable(
                name: "RaceShirtTypes");

            // Remove columns from Registrations table
            migrationBuilder.DropColumn(
                name: "BibName",
                table: "Registrations");

            migrationBuilder.DropColumn(
                name: "DateOfBirth",
                table: "Registrations");

            migrationBuilder.DropColumn(
                name: "RawBirthInput",
                table: "Registrations");

            migrationBuilder.DropColumn(
                name: "ShirtCategory",
                table: "Registrations");

            migrationBuilder.DropColumn(
                name: "ShirtType",
                table: "Registrations");

            // Remove columns from Races table
            migrationBuilder.DropColumn(
                name: "GoogleCredentialPath",
                table: "Races");
        }
    }
}
