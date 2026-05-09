using System;
using System.Collections.Generic;

namespace SmartClassRoom.Web.TempModels;

public partial class QuizQuestion
{
    public int Id { get; set; }

    public int QuizId { get; set; }

    public string QuestionText { get; set; } = null!;

    public string Option1 { get; set; } = null!;

    public string Option2 { get; set; } = null!;

    public string Option3 { get; set; } = null!;

    public string Option4 { get; set; } = null!;

    public int CorrectAnswer { get; set; }

    public decimal Points { get; set; }

    public int OrderIndex { get; set; }

    public string? Explanation { get; set; }

    public virtual Quiz Quiz { get; set; } = null!;
}
