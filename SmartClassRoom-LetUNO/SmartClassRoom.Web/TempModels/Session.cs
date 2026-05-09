using System;
using System.Collections.Generic;

namespace SmartClassRoom.Web.TempModels;

public partial class Session
{
    public int Id { get; set; }

    public int CourseOfferingId { get; set; }

    public DateTime SessionDate { get; set; }

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    public int? RoomId { get; set; }

    public string? SessionType { get; set; }

    public string? Topic { get; set; }

    public string? Status { get; set; }

    public DateTime? StartedAt { get; set; }

    public DateTime? EndedAt { get; set; }

    public int? TimetableId { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? TeacherId { get; set; }

    public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();

    public virtual CourseOffering CourseOffering { get; set; } = null!;

    public virtual Room? Room { get; set; }

    public virtual User? Teacher { get; set; }

    public virtual Timetable? Timetable { get; set; }
}
