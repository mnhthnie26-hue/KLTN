namespace QualityEvalAPI.DTOs;

public record CriteriaGroupRequest(string GroupCode, string GroupName, string? Description);

public record CriteriaGroupResponse(int GroupId, string GroupCode, string GroupName, string? Description, int CriteriaCount);

public record CriteriaRequest(
    int GroupId,
    string CriteriaCode,
    string CriteriaName,
    string? Description,
    decimal Weight,
    decimal ScaleMin,
    decimal ScaleMax,
    bool IsActive
);

public record CriteriaResponse(
    int CriteriaId,
    int GroupId,
    string GroupName,
    string CriteriaCode,
    string CriteriaName,
    string? Description,
    decimal Weight,
    decimal ScaleMin,
    decimal ScaleMax,
    bool IsActive
);
