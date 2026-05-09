using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartClassRoom.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveLatitudeLongitudeFromAttendance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "TeacherAttendances");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "TeacherAttendances");

            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "Attendances");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "Attendances");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Latitude",
                table: "TeacherAttendances",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Longitude",
                table: "TeacherAttendances",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Latitude",
                table: "Attendances",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Longitude",
                table: "Attendances",
                type: "decimal(18,2)",
                nullable: true);
        }
    }
}
