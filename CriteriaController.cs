using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QualityEvalAPI.Data;
using QualityEvalAPI.DTOs;
using QualityEvalAPI.Models;

namespace QualityEvalAPI.Controllers;

[ApiController]
[Route("api/criteria-groups")]
[Authorize]
public class CriteriaGroupController : ControllerBase
{
    private readonly AppDbContext _db;
    public CriteriaGroupController(AppDbContext db) => _db = db;

    // GET api/criteria-groups
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var groups = await _db.CriteriaGroups
            .Select(g => new CriteriaGroupResponse(g.GroupId, g.GroupCode, g.GroupName, g.Description, g.Criteria.Count))
            .ToListAsync();
        return Ok(groups);
    }

    // POST api/criteria-groups
    [HttpPost]
    [Authorize(Roles = "ADMIN,MANAGER")]
    public async Task<IActionResult> Create([FromBody] CriteriaGroupRequest request)
    {
        var group = new CriteriaGroup
        {
            GroupCode = request.GroupCode,
            GroupName = request.GroupName,
            Description = request.Description
        };
        _db.CriteriaGroups.Add(group);
        await _db.SaveChangesAsync();
        return Ok(group);
    }

    // PUT api/criteria-groups/{id}
    [HttpPut("{id:int}")]
    [Authorize(Roles = "ADMIN,MANAGER")]
    public async Task<IActionResult> Update(int id, [FromBody] CriteriaGroupRequest request)
    {
        var group = await _db.CriteriaGroups.FindAsync(id);
        if (group == null) return NotFound();
        group.GroupCode = request.GroupCode;
        group.GroupName = request.GroupName;
        group.Description = request.Description;
        await _db.SaveChangesAsync();
        return Ok(group);
    }

    // DELETE api/criteria-groups/{id}
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "ADMIN,MANAGER")]
    public async Task<IActionResult> Delete(int id)
    {
        var group = await _db.CriteriaGroups.FindAsync(id);
        if (group == null) return NotFound();
        _db.CriteriaGroups.Remove(group);
        await _db.SaveChangesAsync();
        return Ok(new { message = "Đã xóa phân loại tiêu chí." });
    }
}

[ApiController]
[Route("api/criteria")]
[Authorize]
public class CriteriaController : ControllerBase
{
    private readonly AppDbContext _db;
    public CriteriaController(AppDbContext db) => _db = db;

    // GET api/criteria?groupId=1
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? groupId)
    {
        var query = _db.Criteria.Include(c => c.Group).AsQueryable();
        if (groupId.HasValue) query = query.Where(c => c.GroupId == groupId.Value);

        var list = await query.Select(c => new CriteriaResponse(
            c.CriteriaId, c.GroupId, c.Group!.GroupName, c.CriteriaCode, c.CriteriaName,
            c.Description, c.Weight, c.ScaleMin, c.ScaleMax, c.IsActive
        )).ToListAsync();

        return Ok(list);
    }

    // GET api/criteria/{id}
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var c = await _db.Criteria.Include(x => x.Group).FirstOrDefaultAsync(x => x.CriteriaId == id);
        if (c == null) return NotFound();
        return Ok(new CriteriaResponse(c.CriteriaId, c.GroupId, c.Group!.GroupName, c.CriteriaCode,
            c.CriteriaName, c.Description, c.Weight, c.ScaleMin, c.ScaleMax, c.IsActive));
    }

    // POST api/criteria  (Yêu cầu 1: thêm)
    [HttpPost]
    [Authorize(Roles = "ADMIN,MANAGER")]
    public async Task<IActionResult> Create([FromBody] CriteriaRequest request)
    {
        var entity = new Criteria
        {
            GroupId = request.GroupId,
            CriteriaCode = request.CriteriaCode,
            CriteriaName = request.CriteriaName,
            Description = request.Description,
            Weight = request.Weight,
            ScaleMin = request.ScaleMin,
            ScaleMax = request.ScaleMax,
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _db.Criteria.Add(entity);
        await _db.SaveChangesAsync();
        return Ok(entity);
    }

    // PUT api/criteria/{id}  (Yêu cầu 1: sửa)
    [HttpPut("{id:int}")]
    [Authorize(Roles = "ADMIN,MANAGER")]
    public async Task<IActionResult> Update(int id, [FromBody] CriteriaRequest request)
    {
        var entity = await _db.Criteria.FindAsync(id);
        if (entity == null) return NotFound();

        entity.GroupId = request.GroupId;
        entity.CriteriaCode = request.CriteriaCode;
        entity.CriteriaName = request.CriteriaName;
        entity.Description = request.Description;
        entity.Weight = request.Weight;               // Yêu cầu 2: thiết lập trọng số
        entity.ScaleMin = request.ScaleMin;            // Yêu cầu 2: thang điểm
        entity.ScaleMax = request.ScaleMax;
        entity.IsActive = request.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return Ok(entity);
    }

    // DELETE api/criteria/{id}  (Yêu cầu 1: xóa)
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "ADMIN,MANAGER")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _db.Criteria.FindAsync(id);
        if (entity == null) return NotFound();
        _db.Criteria.Remove(entity);
        await _db.SaveChangesAsync();
        return Ok(new { message = "Đã xóa tiêu chí." });
    }
}
