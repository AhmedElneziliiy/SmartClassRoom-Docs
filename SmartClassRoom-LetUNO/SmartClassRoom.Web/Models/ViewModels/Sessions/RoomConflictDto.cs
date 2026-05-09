namespace SmartClassRoom.Web.Models.ViewModels.Sessions
{
    /// <summary>
    /// DTO for detecting and displaying room scheduling conflicts
    /// </summary>
    public class RoomConflictDto
    {
        public int RoomId { get; set; }
        public string RoomName { get; set; } = string.Empty;
        public DateTime ConflictDate { get; set; }
        public List<SessionDisplayDto> ConflictingSessions { get; set; } = new();
        public string ConflictDescription { get; set; } = string.Empty;
    }
}
