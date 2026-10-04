namespace QualityEvalAPI.DTOs;

public record ComputeScoresRequest(int PeriodId, int TargetId, int SurveyId);

public record CriteriaScoreResult(
    int CriteriaId,
    string CriteriaName,
    decimal RawScore,
    decimal Weight,
    decimal WeightedScore
);

public record ComputeScoresResponse(
    int TargetId,
    string TargetName,
    int PeriodId,
    decimal TotalWeightedScore,
    List<CriteriaScoreResult> Details
);

public record AhpPairwiseInput(int CriteriaAId, int CriteriaBId, decimal ComparisonValue);

public record AhpComputeRequest(int GroupId, List<AhpPairwiseInput> Comparisons);

public record AhpWeightResult(int CriteriaId, string CriteriaName, decimal AhpWeight);

public record AhpComputeResponse(
    List<AhpWeightResult> Weights,
    decimal ConsistencyRatio,
    bool IsConsistent
);

public record RankingResult(
    int TargetId,
    string TargetName,
    decimal TotalScore,
    int RankNo,
    string RankLabel
);

public record DashboardSummary(
    int TotalCriteria,
    int TotalSurveys,
    int TotalResponses,
    int TotalUsers,
    decimal AverageScore,
    List<RankingResult> TopRankings,
    List<GroupAverage> GroupAverages
);

public record GroupAverage(string GroupName, decimal AverageScore);
