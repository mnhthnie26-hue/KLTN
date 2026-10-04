namespace QualityEvalAPI.Models;

public class EvaluationPeriod
{
    public int PeriodId { get; set; }
    public string PeriodName { get; set; } = null!;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Status { get; set; } = "OPEN";
}

public class EvaluationTarget
{
    public int TargetId { get; set; }
    public string TargetType { get; set; } = null!;   // DEPARTMENT, LECTURER, COURSE, SERVICE
    public string TargetName { get; set; } = null!;
    public int? RefId { get; set; }
}

public class EvaluationScore
{
    public int ScoreId { get; set; }
    public int PeriodId { get; set; }
    public int TargetId { get; set; }
    public int CriteriaId { get; set; }
    public decimal RawScore { get; set; }
    public decimal WeightedScore { get; set; }
    public string Method { get; set; } = "AVERAGE"; // AVERAGE, AHP, ML
    public DateTime ComputedAt { get; set; } = DateTime.UtcNow;
}

public class AhpMatrix
{
    public int MatrixId { get; set; }
    public int GroupId { get; set; }
    public int CriteriaAId { get; set; }
    public int CriteriaBId { get; set; }
    public decimal ComparisonValue { get; set; }  // thang Saaty 1/9 .. 9
    public int? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class QualityRanking
{
    public int RankingId { get; set; }
    public int PeriodId { get; set; }
    public int TargetId { get; set; }
    public decimal TotalScore { get; set; }
    public int? RankNo { get; set; }
    public string? RankLabel { get; set; }   // Xuất sắc, Tốt, Khá, TB, Yếu
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class Report
{
    public int ReportId { get; set; }
    public int PeriodId { get; set; }
    public string Title { get; set; } = null!;
    public string? FilePath { get; set; }
    public int? GeneratedBy { get; set; }
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}
