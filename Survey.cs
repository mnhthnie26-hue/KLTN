namespace QualityEvalAPI.Models;

public class Survey
{
    public int SurveyId { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public string? TargetRole { get; set; }     // STUDENT / LECTURER / ALL
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Status { get; set; } = "DRAFT"; // DRAFT, OPEN, CLOSED
    public int? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<SurveyQuestion> Questions { get; set; } = new List<SurveyQuestion>();
}

public class SurveyQuestion
{
    public int QuestionId { get; set; }
    public int SurveyId { get; set; }
    public int? CriteriaId { get; set; }
    public string QuestionText { get; set; } = null!;
    public string QuestionType { get; set; } = "SCALE"; // SCALE, TEXT, SINGLE_CHOICE
    public string? OptionsJson { get; set; }
    public int OrderNo { get; set; } = 1;
}

public class SurveyResponse
{
    public int ResponseId { get; set; }
    public int SurveyId { get; set; }
    public int? UserId { get; set; }
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

    public ICollection<SurveyAnswer> Answers { get; set; } = new List<SurveyAnswer>();
}

public class SurveyAnswer
{
    public int AnswerId { get; set; }
    public int ResponseId { get; set; }
    public int QuestionId { get; set; }
    public string? AnswerValue { get; set; }   // điểm số / lựa chọn
    public string? AnswerText { get; set; }    // câu trả lời tự luận
}
