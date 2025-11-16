namespace ShefaafAPI.Services;

public interface ITokenService
{
    string CreateToken(Guid userId, string email, string username, int expiryMinutes, string role = "Customer");
    Guid VerifyTokenAndGetId(string token);
}
