namespace EssenceMvp.Web.Services;

public sealed record RegisterRequest(string Email, string Password, string? DisplayName);
public sealed record LoginRequest(string Email, string Password);
public sealed record AuthResponse(string Token, string RefreshToken, string Email, string? DisplayName);
public sealed record RefreshRequest(string RefreshToken);

public sealed record ProjectDto(int Id, string Name, string? Description, string? Phase, DateTime CreatedAt);
public sealed record CreateProjectRequest(string Name, string? Description, string? Phase);
public sealed record UpdateProjectRequest(string? Name, string? Description, string? Phase);

public sealed record AlphaStatusDto(int AlphaId, string AlphaName, string AreaOfConcern, int CurrentStateNumber, string? CurrentStateName);
public sealed record ProjectDetailDto(int Id, string Name, string? Description, string? Phase, DateTime CreatedAt, List<AlphaStatusDto> AlphaStatuses);

public sealed record AlphaDto(int Id, string Name, string AreaOfConcern, string? Description);
public sealed record AlphaStateDto(int Id, int AlphaId, int StateNumber, string StateName, string? Description);
public sealed record StateChecklistDto(int Id, string CriterionText, bool IsMandatory);
