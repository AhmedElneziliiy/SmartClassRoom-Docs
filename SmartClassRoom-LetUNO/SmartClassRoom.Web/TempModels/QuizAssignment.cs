using System;
using System.Collections.Generic;

namespace SmartClassRoom.Web.TempModels;

public partial class QuizAssignment
{
    public int Id { get; set; }

    public int QuizId { get; set; }

    public int CourseOfferingId { get; set; }

    public DateTime AvailableFrom { get; set; }

    public DateTime AvailableUntil { get; set; }

    public bool IsPublished { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual CourseOffering CourseOffering { get; set; } = null!;

    public virtual Quiz Quiz { get; set; } = null!;

    public virtual ICollection<QuizAttempt> QuizAttempts { get; set; } = new List<QuizAttempt>();
}
