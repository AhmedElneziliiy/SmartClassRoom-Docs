using System;
using System.Collections.Generic;

namespace SmartClassRoom.Web.TempModels;

public partial class ScheduledSlot
{
    public int Id { get; set; }

    public int TimetableId { get; set; }

    public int CourseOfferingId { get; set; }

    public string DayOfWeek { get; set; } = null!;

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    public int? RoomId { get; set; }

    public virtual CourseOffering CourseOffering { get; set; } = null!;

    public virtual Room? Room { get; set; }

    public virtual Timetable Timetable { get; set; } = null!;
}
