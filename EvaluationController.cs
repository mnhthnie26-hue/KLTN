using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QualityEvalAPI.Data;
using QualityEvalAPI.DTOs;
using QualityEvalAPI.Models;
using QualityEvalAPI.Services;

namespace QualityEvalAPI.Controllers;

[ApiController]
[Route("api/evaluation")]
[Authorize(Roles = "ADMIN,MANAGER")]
public class EvaluationController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IAhpService _ahpService;

    public EvaluationController(AppDbContext db, IAhpService ahpService)
    {
        _db = db;
        _ahpService = ahpService;
    }

    // POST api/evaluation/compute
    // Yêu cầu 1: Tính điểm theo từng tiêu chí/KPI dựa trên kết quả khảo sát (phương pháp trung bình có trọng số)
    [HttpPost("compute")]
    public async Task<IActionResult> ComputeScores([FromBody] ComputeScoresRequest request)
    {
        var target = await _db.EvaluationTargets.FindAsync(request.TargetId);
        if (target == null) return NotFound(new { message = "Không tìm thấy đối tượng đánh giá." });

        // Lấy các câu hỏi khảo sát có gắn với tiêu chí
        var questions = await _db.SurveyQuestions
            .Where(q => q.SurveyId == request.SurveyId && q.CriteriaId != null && q.QuestionType == "SCALE")
            .ToListAsync();

        var results = new List<CriteriaScoreResult>();
        decimal totalWeighted = 0;

        foreach (var q in questions)
        {
            var criteria = await _db.Criteria.FindAsync(q.CriteriaId!.Value);
            if (criteria == null) continue;

            // Lấy toàn bộ câu trả lời dạng số cho câu hỏi này
            var values = await _db.SurveyAnswers
                .Where(a => a.QuestionId == q.QuestionId && a.AnswerValue != null)
                .Select(a => a.AnswerValue!)
                .ToListAsync();

            var numericValues = values
                .Select(v => decimal.TryParse(v, out var d) ? (decimal?)d : null)
                .Where(v => v.HasValue)
                .Select(v => v!.Value)
                .ToList();

            if (numericValues.Count == 0) continue;

            var raw = numericValues.Average();
            var weighted = raw * criteria.Weight;
            totalWeighted += weighted;

            results.Add(new CriteriaScoreResult(criteria.CriteriaId, criteria.CriteriaName, Math.Round(raw, 4), criteria.Weight, Math.Round(weighted, 4)));

            // Lưu lại vào evaluation_scores
            _db.EvaluationScores.Add(new EvaluationScore
            {
                PeriodId = request.PeriodId,
                TargetId = request.TargetId,
                CriteriaId = criteria.CriteriaId,
                RawScore = Math.Round(raw, 4),
                WeightedScore = Math.Round(weighted, 4),
                Method = "AVERAGE",
                ComputedAt = DateTime.UtcNow
            });
        }

        await _db.SaveChangesAsync();

        return Ok(new ComputeScoresResponse(request.TargetId, target.TargetName, request.PeriodId, Math.Round(totalWeighted, 4), results));
    }

    // POST api/evaluation/ahp
    // Yêu cầu 2: Áp dụng phương pháp AHP để tính trọng số ưu tiên giữa các tiêu chí trong 1 nhóm
    [HttpPost("ahp")]
    public async Task<IActionResult> ComputeAhp([FromBody] AhpComputeRequest request)
    {
        var criteriaList = await _db.Criteria.Where(c => c.GroupId == request.GroupId).ToListAsync();
        if (criteriaList.Count < 2)
            return BadRequest(new { message = "Cần ít nhất 2 tiêu chí trong nhóm để áp dụng AHP." });

        var ids = criteriaList.Select(c => c.CriteriaId).ToList();
        var names = criteriaList.ToDictionary(c => c.CriteriaId.ToString(), c => c.CriteriaName);

        var result = _ahpService.Compute(ids, names, request.Comparisons);

        // Lưu lại các so sánh cặp đã dùng
        foreach (var c in request.Comparisons)
        {
            _db.AhpMatrices.Add(new AhpMatrix
            {
                GroupId = request.GroupId,
                CriteriaAId = c.CriteriaAId,
                CriteriaBId = c.CriteriaBId,
                ComparisonValue = c.ComparisonValue,
                CreatedAt = DateTime.UtcNow
            });
        }

        // Nếu ma trận nhất quán (CR < 0.1) -> cập nhật lại trọng số cho các tiêu chí
        if (result.IsConsistent)
        {
            foreach (var w in result.Weights)
            {
                var criteria = criteriaList.First(c => c.CriteriaId == w.CriteriaId);
                criteria.Weight = w.AhpWeight;
                criteria.UpdatedAt = DateTime.UtcNow;
            }
        }

        await _db.SaveChangesAsync();
        return Ok(result);
    }

    // GET api/evaluation/scores?periodId=1&targetId=1
    [HttpGet("scores")]
    public async Task<IActionResult> GetScores([FromQuery] int periodId, [FromQuery] int? targetId)
    {
        var query = _db.EvaluationScores.Where(s => s.PeriodId == periodId).AsQueryable();
        if (targetId.HasValue) query = query.Where(s => s.TargetId == targetId.Value);

        var list = await query
            .Join(_db.Criteria, s => s.CriteriaId, c => c.CriteriaId, (s, c) => new
            {
                s.ScoreId,
                s.TargetId,
                s.CriteriaId,
                CriteriaName = c.CriteriaName,
                s.RawScore,
                s.WeightedScore,
                s.Method,
                s.ComputedAt
            }).ToListAsync();

        return Ok(list);
    }
}
