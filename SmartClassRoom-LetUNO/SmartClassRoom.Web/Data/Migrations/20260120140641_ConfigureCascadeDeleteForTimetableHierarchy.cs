using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartClassRoom.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class ConfigureCascadeDeleteForTimetableHierarchy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Attendances_Sessions_SessionId",
                table: "Attendances");

            migrationBuilder.DropForeignKey(
                name: "FK_Sessions_Timetables_TimetableId",
                table: "Sessions");

            migrationBuilder.DropForeignKey(
                name: "FK_TeacherAttendances_Sessions_SessionId",
                table: "TeacherAttendances");

            migrationBuilder.AddForeignKey(
                name: "FK_Attendances_Sessions_SessionId",
                table: "Attendances",
                column: "SessionId",
                principalTable: "Sessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Sessions_Timetables_TimetableId",
                table: "Sessions",
                column: "TimetableId",
                principalTable: "Timetables",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TeacherAttendances_Sessions_SessionId",
                table: "TeacherAttendances",
                column: "SessionId",
                principalTable: "Sessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Attendances_Sessions_SessionId",
                table: "Attendances");

            migrationBuilder.DropForeignKey(
                name: "FK_Sessions_Timetables_TimetableId",
                table: "Sessions");

            migrationBuilder.DropForeignKey(
                name: "FK_TeacherAttendances_Sessions_SessionId",
                table: "TeacherAttendances");

            migrationBuilder.AddForeignKey(
                name: "FK_Attendances_Sessions_SessionId",
                table: "Attendances",
                column: "SessionId",
                principalTable: "Sessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Sessions_Timetables_TimetableId",
                table: "Sessions",
                column: "TimetableId",
                principalTable: "Timetables",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TeacherAttendances_Sessions_SessionId",
                table: "TeacherAttendances",
                column: "SessionId",
                principalTable: "Sessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
