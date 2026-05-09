using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartClassRoom.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class ChangeMaterialUploadedByToApplicationUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Materials_Users_UploadedByTeacherId",
                table: "Materials");

            migrationBuilder.DropColumn(
                name: "UploadedBy",
                table: "Materials");

            migrationBuilder.RenameColumn(
                name: "UploadedByTeacherId",
                table: "Materials",
                newName: "UploadedById");

            migrationBuilder.RenameIndex(
                name: "IX_Materials_UploadedByTeacherId",
                table: "Materials",
                newName: "IX_Materials_UploadedById");

            migrationBuilder.AddForeignKey(
                name: "FK_Materials_Users_UploadedById",
                table: "Materials",
                column: "UploadedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Materials_Users_UploadedById",
                table: "Materials");

            migrationBuilder.RenameColumn(
                name: "UploadedById",
                table: "Materials",
                newName: "UploadedByTeacherId");

            migrationBuilder.RenameIndex(
                name: "IX_Materials_UploadedById",
                table: "Materials",
                newName: "IX_Materials_UploadedByTeacherId");

            migrationBuilder.AddColumn<int>(
                name: "UploadedBy",
                table: "Materials",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_Materials_Users_UploadedByTeacherId",
                table: "Materials",
                column: "UploadedByTeacherId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
