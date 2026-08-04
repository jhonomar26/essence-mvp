namespace EssenceMvp.Application.Dtos;

public record LoginRequest(string Email, string Password);

public record RegisterRequest(string Email, string Password, string? DisplayName);

public record UserResponse(int Id, string Email, string? DisplayName);

public record AuthResponse(string Token, UserResponse User);
