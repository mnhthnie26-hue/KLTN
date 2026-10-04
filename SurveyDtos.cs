namespace QualityEvalAPI.DTOs;

public record SurveyQuestionRequest(
    int? CriteriaId,
    string QuestionText,
    string QuestionType,
    string? OptionsJson,
    int OrderNo
);

public record SurveyRequest(
    string Title,
    string? Description,
    string? TargetRole,
    DateTime? StartDate,
    DateTime? EndDate,
    List<SurveyQuestionRequest> Questions
);

public record SurveyQuestionResponse(
    int QuestionId,
    int? CriteriaId,
    string QuestionText,
    string QuestionType,
    string? OptionsJson,
    int OrderNo
);

public record SurveyResponseDto(
    int SurveyId,
    string Title,
    string? Description,
    string? TargetRole,
    DateTime? StartDate,
    DateTime? EndDate,
    string Status,
    int ResponseCount,
    List<SurveyQuestionResponse> Questions
);

public record SubmitAnswerRequest(int QuestionId, string? AnswerValue, string? AnswerText);

public record SubmitSurveyResponseRequest(List<SubmitAnswerRequest> Answers);
