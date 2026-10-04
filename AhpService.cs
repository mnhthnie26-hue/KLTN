using QualityEvalAPI.DTOs;

namespace QualityEvalAPI.Services;

public class AhpService : IAhpService
{
    // Random Index (RI) - Saaty, theo cỡ ma trận n = 1..10
    private static readonly double[] RandomIndex =
        { 0, 0, 0.58, 0.90, 1.12, 1.24, 1.32, 1.41, 1.45, 1.49 };

    public AhpComputeResponse Compute(List<int> criteriaIds, Dictionary<string, string> criteriaNames, List<AhpPairwiseInput> comparisons)
    {
        int n = criteriaIds.Count;
        if (n < 2)
        {
            return new AhpComputeResponse(new List<AhpWeightResult>(), 0, true);
        }

        // Khởi tạo ma trận vuông n x n, đường chéo = 1
        var matrix = new double[n, n];
        for (int i = 0; i < n; i++) matrix[i, i] = 1.0;

        var indexMap = new Dictionary<int, int>();
        for (int i = 0; i < n; i++) indexMap[criteriaIds[i]] = i;

        foreach (var c in comparisons)
        {
            if (!indexMap.ContainsKey(c.CriteriaAId) || !indexMap.ContainsKey(c.CriteriaBId)) continue;
            int i = indexMap[c.CriteriaAId];
            int j = indexMap[c.CriteriaBId];
            double val = (double)c.ComparisonValue;
            if (val <= 0) val = 1;
            matrix[i, j] = val;
            matrix[j, i] = 1.0 / val;
        }

        // 1. Tính tổng từng cột
        var colSum = new double[n];
        for (int j = 0; j < n; j++)
        {
            for (int i = 0; i < n; i++) colSum[j] += matrix[i, j];
        }

        // 2. Chuẩn hóa ma trận và tính trọng số trung bình (Eigenvector approximation)
        var weights = new double[n];
        for (int i = 0; i < n; i++)
        {
            double rowSum = 0;
            for (int j = 0; j < n; j++)
            {
                rowSum += matrix[i, j] / (colSum[j] == 0 ? 1 : colSum[j]);
            }
            weights[i] = rowSum / n;
        }

        // 3. Tính Lambda-max để đo tính nhất quán
        var weightedSum = new double[n];
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                weightedSum[i] += matrix[i, j] * weights[j];
            }
        }

        double lambdaMax = 0;
        for (int i = 0; i < n; i++)
        {
            if (weights[i] != 0) lambdaMax += weightedSum[i] / weights[i];
        }
        lambdaMax /= n;

        double ci = (lambdaMax - n) / (n - 1); // Consistency Index
        double ri = n - 1 < RandomIndex.Length ? RandomIndex[n - 1] : 1.49;
        double cr = ri == 0 ? 0 : ci / ri;      // Consistency Ratio

        var results = new List<AhpWeightResult>();
        for (int i = 0; i < n; i++)
        {
            var id = criteriaIds[i];
            var name = criteriaNames.TryGetValue(id.ToString(), out var nm) ? nm : $"Tiêu chí {id}";
            results.Add(new AhpWeightResult(id, name, Math.Round((decimal)weights[i], 4)));
        }

        return new AhpComputeResponse(results, Math.Round((decimal)cr, 4), cr < 0.1);
    }
}
