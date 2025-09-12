using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RaceManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EmailQueueSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            

            migrationBuilder.AlterColumn<string>(
                name: "ErrorMessage",
                table: "EmailLogs",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EmailQueueId",
                table: "EmailLogs",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MessageId",
                table: "EmailLogs",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ProcessingTime",
                table: "EmailLogs",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "RecipientEmail",
                table: "EmailLogs",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RecipientName",
                table: "EmailLogs",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Subject",
                table: "EmailLogs",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TemplateName",
                table: "EmailLogs",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "EmailQueues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RegistrationId = table.Column<int>(type: "int", nullable: false),
                    RecipientEmail = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    RecipientName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Subject = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    PlainTextContent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HtmlContent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmailType = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Priority = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ScheduledAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ProcessedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RetryCount = table.Column<int>(type: "int", nullable: false),
                    MaxRetries = table.Column<int>(type: "int", nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    MessageId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Metadata = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailQueues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmailQueues_Registrations_RegistrationId",
                        column: x => x.RegistrationId,
                        principalTable: "Registrations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            
            


            migrationBuilder.CreateIndex(
                name: "IX_EmailLogs_EmailQueueId",
                table: "EmailLogs",
                column: "EmailQueueId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailLogs_MessageId",
                table: "EmailLogs",
                column: "MessageId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailLogs_TemplateName",
                table: "EmailLogs",
                column: "TemplateName");

            migrationBuilder.CreateIndex(
                name: "IX_EmailQueues_EmailType",
                table: "EmailQueues",
                column: "EmailType");

            migrationBuilder.CreateIndex(
                name: "IX_EmailQueues_RegistrationId_EmailType",
                table: "EmailQueues",
                columns: new[] { "RegistrationId", "EmailType" });

            migrationBuilder.CreateIndex(
                name: "IX_EmailQueues_ScheduledAt",
                table: "EmailQueues",
                column: "ScheduledAt");

            migrationBuilder.CreateIndex(
                name: "IX_EmailQueues_Status",
                table: "EmailQueues",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_EmailQueues_Status_ScheduledAt",
                table: "EmailQueues",
                columns: new[] { "Status", "ScheduledAt" });

            
           
           
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            

            migrationBuilder.DropForeignKey(
                name: "FK_Races_GoogleSheetConfigs_SheetConfigId",
                table: "Races");

            migrationBuilder.DropTable(
                name: "EmailQueues");

            migrationBuilder.DropTable(
                name: "GoogleSheetConfigs");

            migrationBuilder.DropTable(
                name: "GoogleCredentials");

            migrationBuilder.DropIndex(
                name: "IX_Races_SheetConfigId",
                table: "Races");

            migrationBuilder.DropIndex(
                name: "IX_EmailLogs_EmailQueueId",
                table: "EmailLogs");

            migrationBuilder.DropIndex(
                name: "IX_EmailLogs_MessageId",
                table: "EmailLogs");

            migrationBuilder.DropIndex(
                name: "IX_EmailLogs_TemplateName",
                table: "EmailLogs");

            migrationBuilder.DropColumn(
                name: "SheetConfigId",
                table: "Races");

            migrationBuilder.DropColumn(
                name: "EmailQueueId",
                table: "EmailLogs");

            migrationBuilder.DropColumn(
                name: "MessageId",
                table: "EmailLogs");

            migrationBuilder.DropColumn(
                name: "ProcessingTime",
                table: "EmailLogs");

            migrationBuilder.DropColumn(
                name: "RecipientEmail",
                table: "EmailLogs");

            migrationBuilder.DropColumn(
                name: "RecipientName",
                table: "EmailLogs");

            migrationBuilder.DropColumn(
                name: "Subject",
                table: "EmailLogs");

            migrationBuilder.DropColumn(
                name: "TemplateName",
                table: "EmailLogs");

            migrationBuilder.AlterColumn<string>(
                name: "ErrorMessage",
                table: "EmailLogs",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);
        }
    }
}
