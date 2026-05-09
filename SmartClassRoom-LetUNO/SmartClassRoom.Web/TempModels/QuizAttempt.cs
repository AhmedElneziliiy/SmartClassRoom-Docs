using System;
using System.Collections.Generic;

namespace SmartClassRoom.Web.TempModels;

public partial class QuizAttempt
{
    public int Id { get; set; }

    public int QuizAssignmentId { get; set; }

    public int StudentId { get; set; }

    public DateTime StartedAt { get; set; }

    public DateTime? SubmittedAt { get; set; }

    public string? Answers { get; set; }

    public string? Results { get; set; }

    public decimal? Score { get; set; }

    public decimal? TotalPoints { get; set; }

    public decimal? Percentage { get; set; }

    public string? Status { get; set; }

    public int AttemptNumber { get; set; }

    public virtual QuizAssignment QuizAssignment { get; set; } = null!;

    public virtual User Student { get; set; } = null!;
}
