using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QualityEvalAPI.Data;
using QualityEvalAPI.DTOs;
using QualityEvalAPI.Models;

namespace QualityEvalAPI.Controllers;

[ApiController]
[Route("api/surveys")]
[Authorize]
public class SurveyController : ControllerBase
{
    private readonly AppDbContext _db;
    public SurveyController(AppDbContext db) => _db = db;

    // GET api/surveys
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? status)
    {
        var query = _db.Surveys.Include(s => s.Questions).AsQueryable();
        if (!string.IsNullOrEmpty(status)) query = query.Where(s => s.Status == status);

        var list = await query.Select(s => new SurveyResponseDto(
            s.SurveyId, s.Title, s.Description, s.TargetRole, s.StartDate, s.EndDate, s.Status,
            _db.SurveyResponses.Count(r => r.SurveyId == s.SurveyId),
            s.Questions.OrderBy(q => q.OrderNo).Select(q =>
                new SurveyQuestionResponse(q.QuestionId, q.CriteriaId, q.QuestionText, q.QuestionType, q.OptionsJson, q.OrderNo)
            ).ToList()
        )).ToListAsync();

        return Ok(list);
    }

    // GET api/surveys/{id}
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var s = await _db.Surveys.Include(x => x.Questions).FirstOrDefaultAsync(x => x.SurveyId == id);
        if (s == null) return NotFound();

        var responseCount = await _db.SurveyResponses.CountAsync(r => r.SurveyId == id);
        return Ok(new SurveyResponseDto(
            s.SurveyId, s.Title, s.Description, s.TargetRole, s.StartDate, s.EndDate, s.Status, responseCount,
            s.Questions.OrderBy(q => q.OrderNo).Select(q =>
                new SurveyQuestionResponse(q.QuestionId, q.CriteriaId, q.QuestionText, q.QuestionType, q.OptionsJson, q.OrderNo)
            ).ToList()
        ));
    }

    // POST api/surveys  (Yêu cầu 1: tạo phiếu khảo sát)
    [HttpPost]
    [Authorize(Roles = "ADMIN,MANAGER")]
    public async Task<IActionResult> Create([FromBody] SurveyRequest request)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var survey = new Survey
        {
            Title = request.Title,
            Description = request.Description,
            TargetRole = request.TargetRole,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Status = "DRAFT",
            CreatedBy = userId,
            CreatedAt = DateTime.UtcNow
        };

        foreach (var q in request.Questions)
        {
            survey.Questions.Add(new SurveyQuestion
            {
                CriteriaId = q.CriteriaId,
                QuestionText = q.QuestionText,
                QuestionType = q.QuestionType,
                OptionsJson = q.OptionsJson,
                OrderNo = q.OrderNo
            });
        }

        _db.Surveys.Add(survey);
        await _db.SaveChangesAsync();
        return Ok(new { survey.SurveyId, message = "Tạo phiếu khảo sát thành công." });
    }

    // PUT api/surveys/{id}/status   (mở/đóng khảo sát - Yêu cầu 1: quản lý)
    [HttpPut("{id:int}/status")]
    [Authorize(Roles = "ADMIN,MANAGER")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] string status)
    {
        var survey = await _db.Surveys.FindAsync(id);
        if (survey == null) return NotFound();
        if (!new[] { "DRAFT", "OPEN", "CLOSED" }.Contains(status)) return BadRequest("Trạng thái không hợp lệ.");
        survey.Status = status;
        await _db.SaveChangesAsync();
        return Ok(survey);
    }

    // DELETE api/surveys/{id}
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "ADMIN,MANAGER")]
    public async Task<IActionResult> Delete(int id)
    {
        var survey = await _db.Surveys.FindAsync(id);
        if (survey == null) return NotFound();
        _db.Surveys.Remove(survey);
        await _db.SaveChangesAsync();
        return Ok(new { message = "Đã xóa khảo sát." });
    }

    // POST api/surveys/{id}/responses  (Yêu cầu 2: thu thập, lưu trữ kết quả khảo sát)
    [HttpPost("{id:int}/responses")]
    public async Task<IActionResult> SubmitResponse(int id, [FromBody] SubmitSurveyResponseRequest request)
    {
        var survey = await _db.Surveys.FindAsync(id);
        if (survey == null) return NotFound();
        if (survey.Status != "OPEN") return BadRequest(new { message = "Khảo sát hiện không mở để tiếp nhận phản hồi." });

        int? userId = null;
        if (User.Identity?.IsAuthenticated == true)
        {
            userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        }

        var response = new SurveyResponse
        {
            SurveyId = id,
            UserId = userId,
            SubmittedAt = DateTime.UtcNow
        };

        foreach (var a in request.Answers)
        {
            response.Answers.Add(new SurveyAnswer
            {
                QuestionId = a.QuestionId,
                AnswerValue = a.AnswerValue,
                AnswerText = a.AnswerText
            });
        }

        _db.SurveyResponses.Add(response);
        await _db.SaveChangesAsync();
        return Ok(new { message = "Ghi nhận phản hồi khảo sát thành công.", responseId = response.ResponseId });
    }

    // GET api/surveys/{id}/responses  (xem kết quả thô đã lưu trữ)
    [HttpGet("{id:int}/responses")]
    [Authorize(Roles = "ADMIN,MANAGER,LECTURER")]
    public async Task<IActionResult> GetResponses(int id)
    {
        var responses = await _db.SurveyResponses
            .Where(r => r.SurveyId == id)
            .Include(r => r.Answers)
            .Select(r => new
            {
                r.ResponseId,
                r.UserId,
                r.SubmittedAt,
                Answers = r.Answers.Select(a => new { a.QuestionId, a.AnswerValue, a.AnswerText })
            }).ToListAsync();

        return Ok(responses);
    }
}
