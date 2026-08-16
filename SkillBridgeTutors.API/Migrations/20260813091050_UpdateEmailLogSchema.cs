using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SkillBridgeTutors.API.Migrations
{
    /// <inheritdoc />
    public partial class UpdateEmailLogSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ToAddress",
                table: "EmailLogs",
                newName: "ToEmail");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "EmailLogs",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<long>(
                name: "LeadId",
                table: "EmailLogs",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProviderMessageId",
                table: "EmailLogs",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "EmailLogs");

            migrationBuilder.DropColumn(
                name: "LeadId",
                table: "EmailLogs");

            migrationBuilder.DropColumn(
                name: "ProviderMessageId",
                table: "EmailLogs");

            migrationBuilder.RenameColumn(
                name: "ToEmail",
                table: "EmailLogs",
                newName: "ToAddress");
        }
    }
}
