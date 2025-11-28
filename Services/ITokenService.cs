namespace ShefaafAPI.Services;

public interface ITokenService
{
    string CreateToken(Guid userId, string email, string username, string role, string phoneNumber);
    Guid? VerifyTokenAndGetId(string token);
}
