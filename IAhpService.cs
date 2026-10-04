using QualityEvalAPI.DTOs;

namespace QualityEvalAPI.Services;

public interface IAhpService
{
    /// <summary>
    /// Tính trọng số ưu tiên (AHP weights) và tỷ số nhất quán (Consistency Ratio)
    /// từ ma trận so sánh cặp theo thang đo Saaty (1/9 .. 9).
    /// </summary>
    AhpComputeResponse Compute(List<int> criteriaIds, Dictionary<string, string> criteriaNames, List<AhpPairwiseInput> comparisons);
}
