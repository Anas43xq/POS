namespace POS.Api.Auth;

public record LoginRequest(string Username, string Password);

public record LoginResponse(string Token, string RefreshToken, string Username, string RoleName, string FullName);

public record RefreshRequest(string RefreshToken);

public record RefreshResponse(string Token);