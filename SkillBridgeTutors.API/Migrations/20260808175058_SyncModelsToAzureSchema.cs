using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SkillBridgeTutors.API.Migrations
{
    /// <inheritdoc />
    public partial class SyncModelsToAzureSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CallRecords_Leads_LeadId",
                table: "CallRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_DemoBookings_DemoSlots_DemoSlotId",
                table: "DemoBookings");

            migrationBuilder.DropForeignKey(
                name: "FK_DemoBookings_Leads_LeadId",
                table: "DemoBookings");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Leads",
                table: "Leads");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DemoSlots",
                table: "DemoSlots");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DemoBookings",
                table: "DemoBookings");

            migrationBuilder.DropIndex(
                name: "IX_DemoBookings_DemoSlotId",
                table: "DemoBookings");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CallRecords",
                table: "CallRecords");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "DemoSlots");

            migrationBuilder.DropColumn(
                name: "TutorName",
                table: "DemoSlots");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "DemoBookings");

            migrationBuilder.DropColumn(
                name: "Curriculum",
                table: "DemoBookings");

            migrationBuilder.DropColumn(
                name: "DemoSlotId",
                table: "DemoBookings");

            migrationBuilder.DropColumn(
                name: "Grade",
                table: "DemoBookings");

            migrationBuilder.DropColumn(
                name: "StudentName",
                table: "DemoBookings");

            migrationBuilder.DropColumn(
                name: "Subject",
                table: "DemoBookings");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "CallRecords");

            migrationBuilder.DropColumn(
                name: "Transcript",
                table: "CallRecords");

            migrationBuilder.RenameColumn(
                name: "ParentName",
                table: "Leads",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "CallStatus",
                table: "Leads",
                newName: "Source");

            migrationBuilder.RenameColumn(
                name: "SlotDateTime",
                table: "DemoSlots",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "IsBooked",
                table: "DemoSlots",
                newName: "IsAvailable");

            migrationBuilder.AddColumn<long>(
                name: "LeadId",
                table: "Leads",
                type: "bigint",
                nullable: false,
                defaultValue: 0L)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<string>(
                name: "FullName",
                table: "Leads",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Leads",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<long>(
                name: "SlotId",
                table: "DemoSlots",
                type: "bigint",
                nullable: false,
                defaultValue: 0L)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "DemoSlots",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "EndTime",
                table: "DemoSlots",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "StartTime",
                table: "DemoSlots",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<long>(
                name: "LeadId",
                table: "DemoBookings",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<long>(
                name: "BookingId",
                table: "DemoBookings",
                type: "bigint",
                nullable: false,
                defaultValue: 0L)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<string>(
                name: "CancellationReason",
                table: "DemoBookings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CancelledAt",
                table: "DemoBookings",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "DemoBookings",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<long>(
                name: "RescheduledFromBookingId",
                table: "DemoBookings",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "SlotId",
                table: "DemoBookings",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "DemoBookings",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<long>(
                name: "LeadId",
                table: "CallRecords",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<long>(
                name: "CallRecordId",
                table: "CallRecords",
                type: "bigint",
                nullable: false,
                defaultValue: 0L)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<string>(
                name: "CallDirection",
                table: "CallRecords",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "CallRecords",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "StartedAt",
                table: "CallRecords",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "CallRecords",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddPrimaryKey(
                name: "PK_Leads",
                table: "Leads",
                column: "LeadId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DemoSlots",
                table: "DemoSlots",
                column: "SlotId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DemoBookings",
                table: "DemoBookings",
                column: "BookingId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CallRecords",
                table: "CallRecords",
                column: "CallRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_DemoBookings_SlotId",
                table: "DemoBookings",
                column: "SlotId");

            migrationBuilder.AddForeignKey(
                name: "FK_CallRecords_Leads_LeadId",
                table: "CallRecords",
                column: "LeadId",
                principalTable: "Leads",
                principalColumn: "LeadId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DemoBookings_DemoSlots_SlotId",
                table: "DemoBookings",
                column: "SlotId",
                principalTable: "DemoSlots",
                principalColumn: "SlotId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DemoBookings_Leads_LeadId",
                table: "DemoBookings",
                column: "LeadId",
                principalTable: "Leads",
                principalColumn: "LeadId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CallRecords_Leads_LeadId",
                table: "CallRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_DemoBookings_DemoSlots_SlotId",
                table: "DemoBookings");

            migrationBuilder.DropForeignKey(
                name: "FK_DemoBookings_Leads_LeadId",
                table: "DemoBookings");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Leads",
                table: "Leads");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DemoSlots",
                table: "DemoSlots");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DemoBookings",
                table: "DemoBookings");

            migrationBuilder.DropIndex(
                name: "IX_DemoBookings_SlotId",
                table: "DemoBookings");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CallRecords",
                table: "CallRecords");

            migrationBuilder.DropColumn(
                name: "LeadId",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "FullName",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "SlotId",
                table: "DemoSlots");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "DemoSlots");

            migrationBuilder.DropColumn(
                name: "EndTime",
                table: "DemoSlots");

            migrationBuilder.DropColumn(
                name: "StartTime",
                table: "DemoSlots");

            migrationBuilder.DropColumn(
                name: "BookingId",
                table: "DemoBookings");

            migrationBuilder.DropColumn(
                name: "CancellationReason",
                table: "DemoBookings");

            migrationBuilder.DropColumn(
                name: "CancelledAt",
                table: "DemoBookings");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "DemoBookings");

            migrationBuilder.DropColumn(
                name: "RescheduledFromBookingId",
                table: "DemoBookings");

            migrationBuilder.DropColumn(
                name: "SlotId",
                table: "DemoBookings");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "DemoBookings");

            migrationBuilder.DropColumn(
                name: "CallRecordId",
                table: "CallRecords");

            migrationBuilder.DropColumn(
                name: "CallDirection",
                table: "CallRecords");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "CallRecords");

            migrationBuilder.DropColumn(
                name: "StartedAt",
                table: "CallRecords");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "CallRecords");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "Leads",
                newName: "ParentName");

            migrationBuilder.RenameColumn(
                name: "Source",
                table: "Leads",
                newName: "CallStatus");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "DemoSlots",
                newName: "SlotDateTime");

            migrationBuilder.RenameColumn(
                name: "IsAvailable",
                table: "DemoSlots",
                newName: "IsBooked");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "Leads",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "DemoSlots",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<string>(
                name: "TutorName",
                table: "DemoSlots",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "LeadId",
                table: "DemoBookings",
                type: "int",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "DemoBookings",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<string>(
                name: "Curriculum",
                table: "DemoBookings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "DemoSlotId",
                table: "DemoBookings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Grade",
                table: "DemoBookings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "StudentName",
                table: "DemoBookings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Subject",
                table: "DemoBookings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<int>(
                name: "LeadId",
                table: "CallRecords",
                type: "int",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "CallRecords",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<string>(
                name: "Transcript",
                table: "CallRecords",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Leads",
                table: "Leads",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DemoSlots",
                table: "DemoSlots",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DemoBookings",
                table: "DemoBookings",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CallRecords",
                table: "CallRecords",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_DemoBookings_DemoSlotId",
                table: "DemoBookings",
                column: "DemoSlotId");

            migrationBuilder.AddForeignKey(
                name: "FK_CallRecords_Leads_LeadId",
                table: "CallRecords",
                column: "LeadId",
                principalTable: "Leads",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DemoBookings_DemoSlots_DemoSlotId",
                table: "DemoBookings",
                column: "DemoSlotId",
                principalTable: "DemoSlots",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DemoBookings_Leads_LeadId",
                table: "DemoBookings",
                column: "LeadId",
                principalTable: "Leads",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
