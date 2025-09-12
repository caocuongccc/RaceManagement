using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RaceManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CredentialManagementSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create GoogleCredentials table
            migrationBuilder.CreateTable(
                name: "GoogleCredentials",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ServiceAccountEmail = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    CredentialFilePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GoogleCredentials", x => x.Id);
                });

            // Create GoogleSheetConfigs table
            migrationBuilder.CreateTable(
                name: "GoogleSheetConfigs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SpreadsheetId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    SheetName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CredentialId = table.Column<int>(type: "int", nullable: false),
                    HeaderRowIndex = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    DataStartRowIndex = table.Column<int>(type: "int", nullable: false, defaultValue: 2),
                    LastSyncRowIndex = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GoogleSheetConfigs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GoogleSheetConfigs_GoogleCredentials_CredentialId",
                        column: x => x.CredentialId,
                        principalTable: "GoogleCredentials",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict); // Don't cascade delete
                });

            // Add SheetConfigId to Races table
            migrationBuilder.AddColumn<int>(
                name: "SheetConfigId",
                table: "Races",
                type: "int",
                nullable: true);

            // Create indexes for performance
            migrationBuilder.CreateIndex(
                name: "IX_GoogleCredentials_Name",
                table: "GoogleCredentials",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GoogleCredentials_ServiceAccountEmail",
                table: "GoogleCredentials",
                column: "ServiceAccountEmail",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GoogleCredentials_IsActive",
                table: "GoogleCredentials",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_GoogleSheetConfigs_CredentialId",
                table: "GoogleSheetConfigs",
                column: "CredentialId");

            migrationBuilder.CreateIndex(
                name: "IX_GoogleSheetConfigs_SpreadsheetId",
                table: "GoogleSheetConfigs",
                column: "SpreadsheetId");

            migrationBuilder.CreateIndex(
                name: "IX_GoogleSheetConfigs_IsActive",
                table: "GoogleSheetConfigs",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_GoogleSheetConfigs_Name_CredentialId",
                table: "GoogleSheetConfigs",
                columns: new[] { "Name", "CredentialId" },
                unique: true);

            // Create foreign key for Races -> GoogleSheetConfigs
            migrationBuilder.CreateIndex(
                name: "IX_Races_SheetConfigId",
                table: "Races",
                column: "SheetConfigId");

            migrationBuilder.AddForeignKey(
                name: "FK_Races_GoogleSheetConfigs_SheetConfigId",
                table: "Races",
                column: "SheetConfigId",
                principalTable: "GoogleSheetConfigs",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull); // Set null when config deleted
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop foreign key and column from Races
            migrationBuilder.DropForeignKey(
                name: "FK_Races_GoogleSheetConfigs_SheetConfigId",
                table: "Races");

            migrationBuilder.DropIndex(
                name: "IX_Races_SheetConfigId",
                table: "Races");

            migrationBuilder.DropColumn(
                name: "SheetConfigId",
                table: "Races");

            // Drop GoogleSheetConfigs table
            migrationBuilder.DropTable(
                name: "GoogleSheetConfigs");

            // Drop GoogleCredentials table
            migrationBuilder.DropTable(
                name: "GoogleCredentials");
        }
    }
}
