using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShefaafAPI.Models;
using ShefaafAPI.Models.DTOs;
using ShefaafAPI.Services;

namespace ShefaafAPI.Controllers.v1;

[Route("v1/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly ISqlService _sqlService;
    private readonly ITokenService _tokenService;
    private readonly IMailService _mailService;
    private readonly ILogger<UserController> _logger;

    public UserController(
        ISqlService sqlService, 
        ITokenService tokenService, 
        IMailService mailService,
        ILogger<UserController> logger)
    {
        _sqlService = sqlService;
        _tokenService = tokenService;
        _mailService = mailService;
        _logger = logger;
    }



    // TEMPORARY - Only for first admin setup
[HttpPost("Setup/FirstAdmin")]
[AllowAnonymous]
public async Task<IActionResult> SetupFirstAdmin([FromBody] string email)
{
    try
    {
        var user = await _sqlService.FindUser(email);
        if (user == null)
        {
            return NotFound(new { success = false, message = "User not found" });
        }

        await _sqlService.UpdateUserRole(user.UserId, "Admin");

        return Ok(new
        {
            success = true,
            message = "User is now Admin! Login again to get admin token."
        });
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error setting up first admin");
        return StatusCode(500, new { success = false, message = "Server Error" });
    }
}


    [Authorize(Roles = "Admin")]
    [HttpPost("MakeAdmin")]
    public async Task<IActionResult> MakeAdmin([FromBody] string email)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return BadRequest(new { success = false, message = "Email is required" });
            }

            var user = await _sqlService.FindUser(email);
            if (user == null)
            {
                return NotFound(new { success = false, message = "User not found" });
            }

            await _sqlService.UpdateUserRole(user.UserId, "Admin");

            return Ok(new
            {
                success = true,
                message = "User is now Admin! They must login again to get new admin token."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error making user admin: {Email}", email);
            return StatusCode(500, new { success = false, message = "Server Error" });
        }
    }

    [HttpPost("Register")]
    public async Task<IActionResult> Register(RegisterRequest model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, message = "Invalid input data" });
            }

            var existingUser = await _sqlService.FindUser(model.Email);

            if (existingUser != null)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "User Already Exists!"
                });
            }

            var encryptedPass = BCrypt.Net.BCrypt.EnhancedHashPassword(model.Password);

            var user = new User
            {
                UserId = Guid.NewGuid(),
                Username = model.Username,
                Email = model.Email,
                Password = encryptedPass,
                PhoneNumber = model.PhoneNumber,
                Address = model.Address,
                City = model.City,
                PinCode = model.PinCode
            };

            await _sqlService.CreateUser(user);

            return Ok(new
            {
                success = true,
                message = "User Created Successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration for email: {Email}", model.Email);
            return StatusCode(500, new { success = false, message = "Server Error" });
        }
    }

    [HttpPost("Login")]
    public async Task<IActionResult> Login(LoginRequest model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, message = "Invalid input data" });
            }

            var existingUser = await _sqlService.FindUser(model.Email);

            if (existingUser == null)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Invalid email or password"
                });
            }

            var checkPass = BCrypt.Net.BCrypt.EnhancedVerify(model.Password, existingUser.Password);
            
            if (!checkPass)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Invalid email or password"
                });
            }

            var token = _tokenService.CreateToken(
                existingUser.UserId, 
                existingUser.Email, 
                existingUser.Username, 
                existingUser.Role,
                existingUser.PhoneNumber
            );
            
            return Ok(new
            {
                success = true,
                message = "Logged In Successfully!",
                token,
                role = existingUser.Role,
                username = existingUser.Username,
                email = existingUser.Email
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for email: {Email}", model.Email);
            return StatusCode(500, new { success = false, message = "Server Error" });
        }
    }

    [Authorize]
    [HttpDelete("Delete")]
    public async Task<IActionResult> DeleteAccount(LoginRequest model)
    {
        try
        {
            var user = await _sqlService.FindUser(model.Email);

            if (user == null)
            {
                return BadRequest(new { success = false, message = "No User Found" });
            }
            
            var passVerify = BCrypt.Net.BCrypt.EnhancedVerify(model.Password, user.Password);

            if (!passVerify)
            {
                return BadRequest(new { success = false, message = "Password incorrect" });
            }

            await _sqlService.DeleteUser(model.Email);

            return Ok(new
            {
                success = true,
                message = "User Account Deleted successfully!"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting account for email: {Email}", model.Email);
            return StatusCode(500, new { success = false, message = "Server Error!" });
        }
    }

    [HttpPost("Forgot-pass")]
    public async Task<IActionResult> ForgotPass(EmailRequest model)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(model.EMail))
            {
                return BadRequest(new { success = false, message = "Email is required" });
            }

            var findUser = await _sqlService.FindUser(model.EMail);

            if (findUser == null)
            {
                return Ok(new
                {
                    success = true,
                    message = "If the email exists, a password reset link has been sent"
                });
            }
            
            var token = _tokenService.CreateToken(
                findUser.UserId, 
                model.EMail, 
                findUser.Username, 
                findUser.Role,
                findUser.PhoneNumber
            );

            string link = $"http://localhost:3000/reset-password?token={token}";
            
            await _mailService.SendEmailAsync(
                model.EMail, 
                "Password Reset Request", 
                $"Click here to reset your password: {link}<br>This link expires in 1 hour.", 
                true
            );

            return Ok(new
            {
                success = true,
                message = "If the email exists, a password reset link has been sent"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in forgot password for email: {Email}", model.EMail);
            return StatusCode(500, new { success = false, message = "Server Error" });
        }
    }

    [HttpPost("Change-password")]
    public async Task<IActionResult> ChangePassword(string token, ChangePassRequest updationReq)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return BadRequest(new { success = false, message = "Token is required" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, message = "Invalid input data" });
            }

            var userId = _tokenService.VerifyTokenAndGetId(token);

            if (userId == null)
            {
                return Unauthorized(new { success = false, message = "Invalid or expired token" });
            }

            if (updationReq.Password != updationReq.ConfirmPassword)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Password does not match"
                });
            }

            var encryptedPass = BCrypt.Net.BCrypt.EnhancedHashPassword(updationReq.Password);

            var updatePass = await _sqlService.UpdatePass(userId.Value, encryptedPass);

            if (updatePass)
            {
                return Ok(new
                {
                    success = true,
                    message = "Password updated Successfully"
                });
            }
            else
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Some Error during updating password!"
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing password");
            
            if (ex.Message.Contains("Token has expired"))
            {
                return Unauthorized(new { success = false, message = "Password Reset Link Expired!" });
            }
            
            return StatusCode(500, new { success = false, message = "Server Error" });
        }
    }
}
