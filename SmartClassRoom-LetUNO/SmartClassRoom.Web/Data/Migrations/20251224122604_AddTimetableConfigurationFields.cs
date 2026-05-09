using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartClassRoom.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTimetableConfigurationFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<TimeSpan>(
                name: "EndTime",
                table: "Timetables",
                type: "time",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Timetables",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "SlotDurationMinutes",
                table: "Timetables",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "StartTime",
                table: "Timetables",
                type: "time",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddColumn<string>(
                name: "WorkingDaysJson",
                table: "Timetables",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EndTime",
                table: "Timetables");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Timetables");

            migrationBuilder.DropColumn(
                name: "SlotDurationMinutes",
                table: "Timetables");

            migrationBuilder.DropColumn(
                name: "StartTime",
                table: "Timetables");

            migrationBuilder.DropColumn(
                name: "WorkingDaysJson",
                table: "Timetables");
        }
    }
}
