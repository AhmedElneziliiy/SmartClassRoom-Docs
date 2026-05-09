using System;
using System.Collections.Generic;

namespace SmartClassRoom.Web.TempModels;

public partial class Quiz
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public int CourseId { get; set; }

    public int TimeLimit { get; set; }

    public decimal PassingScore { get; set; }

    public int? MaxAttempts { get; set; }

    public bool ShuffleQuestions { get; set; }

    public bool ShowCorrectAnswers { get; set; }

    public int CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public bool IsActive { get; set; }

    public int CreatorId { get; set; }

    public virtual Course Course { get; set; } = null!;

    public virtual User Creator { get; set; } = null!;

    public virtual ICollection<QuizAssignment> QuizAssignments { get; set; } = new List<QuizAssignment>();

    public virtual ICollection<QuizQuestion> QuizQuestions { get; set; } = new List<QuizQuestion>();
}
