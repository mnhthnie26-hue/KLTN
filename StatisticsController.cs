using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QualityEvalAPI.Data;
using QualityEvalAPI.DTOs;
using QualityEvalAPI.Models;

namespace QualityEvalAPI.Controllers;

[ApiController]
[Route("api/statistics")]
[Authorize]
public class StatisticsController : ControllerBase
{
    private readonly AppDbContext _db;
    public StatisticsController(AppDbContext db) => _db = db;

    // GET api/statistics/dashboard
    // Yêu cầu 1: Hiển thị bảng, biểu đồ và dashboard kết quả
    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard([FromQuery] int? periodId)
    {
        var totalCriteria = await _db.Criteria.CountAsync(c => c.IsActive);
        var totalSurveys = await _db.Surveys.CountAsync();
        var totalResponses = await _db.SurveyResponses.CountAsync();
        var totalUsers = await _db.Users.CountAsync(u => u.Status);

        var scoreQuery = _db.EvaluationScores.AsQueryable();
        if (periodId.HasValue) scoreQuery = scoreQuery.Where(s => s.PeriodId == periodId.Value);

        var avgScore = await scoreQuery.AnyAsync()
            ? Math.Round(await scoreQuery.AverageAsync(s => s.WeightedScore), 2)
            : 0m;

        // Điểm trung bình theo nhóm tiêu chí
        var groupAverages = await scoreQuery
            .Join(_db.Criteria, s => s.CriteriaId, c => c.CriteriaId, (s, c) => new { s, c })
            .Join(_db.CriteriaGroups, sc => sc.c.GroupId, g => g.GroupId, (sc, g) => new { sc.s, GroupName = g.GroupName })
            .GroupBy(x => x.GroupName)
            .Select(g => new GroupAverage(g.Key, Math.Round(g.Average(x => x.s.WeightedScore), 2)))
            .ToListAsync();

        // Top xếp hạng gần nhất
        var topRankings = await _db.QualityRankings
            .Where(r => !periodId.HasValue || r.PeriodId == periodId.Value)
            .OrderBy(r => r.RankNo)
            .Take(10)
            .Join(_db.EvaluationTargets, r => r.TargetId, t => t.TargetId, (r, t) =>
                new RankingResult(t.TargetId, t.TargetName, r.TotalScore, r.RankNo ?? 0, r.RankLabel ?? ""))
            .ToListAsync();

        return Ok(new DashboardSummary(totalCriteria, totalSurveys, totalResponses, totalUsers, avgScore, topRankings, groupAverages));
    }

    // POST api/statistics/rankings/{periodId}/generate
    // Yêu cầu 2: Xếp hạng chất lượng dựa trên tổng điểm có trọng số của mỗi đối tượng
    [HttpPost("rankings/{periodId:int}/generate")]
    [Authorize(Roles = "ADMIN,MANAGER")]
    public async Task<IActionResult> GenerateRankings(int periodId)
    {
        var totals = await _db.EvaluationScores
            .Where(s => s.PeriodId == periodId)
            .GroupBy(s => s.TargetId)
            .Select(g => new { TargetId = g.Key, Total = g.Sum(x => x.WeightedScore) })
            .OrderByDescending(x => x.Total)
            .ToListAsync();

        // Xóa xếp hạng cũ của kỳ này
        var old = _db.QualityRankings.Where(r => r.PeriodId == periodId);
        _db.QualityRankings.RemoveRange(old);

        int rank = 1;
        var rankings = new List<QualityRanking>();
        foreach (var t in totals)
        {
            string label = t.Total >= 4.5m ? "Xuất sắc"
                : t.Total >= 3.5m ? "Tốt"
                : t.Total >= 2.5m ? "Khá"
                : t.Total >= 1.5m ? "Trung bình" : "Yếu";

            rankings.Add(new QualityRanking
            {
                PeriodId = periodId,
                TargetId = t.TargetId,
                TotalScore = Math.Round(t.Total, 4),
                RankNo = rank++,
                RankLabel = label,
                CreatedAt = DateTime.UtcNow
            });
        }

        _db.QualityRankings.AddRange(rankings);
        await _db.SaveChangesAsync();

        return Ok(new { message = "Đã tạo bảng xếp hạng chất lượng.", count = rankings.Count });
    }

    // GET api/statistics/rankings/{periodId}
    [HttpGet("rankings/{periodId:int}")]
    public async Task<IActionResult> GetRankings(int periodId)
    {
        var list = await _db.QualityRankings
            .Where(r => r.PeriodId == periodId)
            .OrderBy(r => r.RankNo)
            .Join(_db.EvaluationTargets, r => r.TargetId, t => t.TargetId, (r, t) =>
                new RankingResult(t.TargetId, t.TargetName, r.TotalScore, r.RankNo ?? 0, r.RankLabel ?? ""))
            .ToListAsync();

        return Ok(list);
    }

    // GET api/statistics/reports/{periodId}/export-csv
    // Yêu cầu 2: xuất báo cáo (CSV - có thể mở bằng Excel)
    [HttpGet("reports/{periodId:int}/export-csv")]
    [Authorize(Roles = "ADMIN,MANAGER")]
    public async Task<IActionResult> ExportCsv(int periodId)
    {
        var rankings = await _db.QualityRankings
            .Where(r => r.PeriodId == periodId)
            .OrderBy(r => r.RankNo)
            .Join(_db.EvaluationTargets, r => r.TargetId, t => t.TargetId, (r, t) =>
                new { t.TargetName, r.TotalScore, r.RankNo, r.RankLabel })
            .ToListAsync();

        var sb = new System.Text.StringBuilder();
        sb.AppendLine("Hạng,Đối tượng,Tổng điểm,Xếp loại");
        foreach (var r in rankings)
        {
            sb.AppendLine($"{r.RankNo},{r.TargetName},{r.TotalScore},{r.RankLabel}");
        }

        var bytes = System.Text.Encoding.UTF8.GetPreamble().Concat(System.Text.Encoding.UTF8.GetBytes(sb.ToString())).ToArray();

        _db.Reports.Add(new Report
        {
            PeriodId = periodId,
            Title = $"Bao cao xep hang chat luong - Ky {periodId}",
            FilePath = null,
            GeneratedAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();

        return File(bytes, "text/csv", $"bao-cao-xep-hang-ky-{periodId}.csv");
    }
}
