namespace QualityEvalAPI.Models;

public class CriteriaGroup
{
    public int GroupId { get; set; }
    public string GroupCode { get; set; } = null!;
    public string GroupName { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Criteria> Criteria { get; set; } = new List<Criteria>();
}

public class Criteria
{
    public int CriteriaId { get; set; }
    public int GroupId { get; set; }
    public CriteriaGroup? Group { get; set; }
    public string CriteriaCode { get; set; } = null!;
    public string CriteriaName { get; set; } = null!;
    public string? Description { get; set; }
    public decimal Weight { get; set; } = 1.0m;      // trọng số
    public decimal ScaleMin { get; set; } = 1;
    public decimal ScaleMax { get; set; } = 5;
    public bool IsActive { get; set; } = true;
    public int? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
