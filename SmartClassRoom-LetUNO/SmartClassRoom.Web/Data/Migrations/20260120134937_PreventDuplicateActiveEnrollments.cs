using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartClassRoom.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class PreventDuplicateActiveEnrollments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create a unique filtered index to prevent duplicate active enrollments
            // This allows only ONE 'Enrolled' record per student+course combination
            // Multiple 'Completed' or 'Dropped' records are still allowed for history
            migrationBuilder.Sql(@"
                CREATE UNIQUE NONCLUSTERED INDEX IX_StudentEnrollments_StudentId_CourseOfferingId_Active
                ON StudentEnrollments (StudentId, CourseOfferingId)
                WHERE Status = 'Enrolled';
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DROP INDEX IX_StudentEnrollments_StudentId_CourseOfferingId_Active
                ON StudentEnrollments;
            ");
        }
    }
}
