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

    public UserController(ISqlService sqlService, ITokenService tokenService, IMailService mailService)
    {
        _sqlService = sqlService;
        _tokenService = tokenService;
        _mailService = mailService;
    }

    [HttpPost("MakeAdmin")]
    public async Task<IActionResult> MakeAdmin([FromBody] string email)
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
                message = "User is now Admin! Login again to get new admin token."
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Server Error", error = ex.Message });
        }
    }

    [HttpPost("Register")]
    public async Task<IActionResult> Register(RegisterRequest model)
    {
        try
        {
            var existingUser = await _sqlService.FindUser(model.Email);

            if (existingUser == null)
            {
                var encryptedPass = BCrypt.Net.BCrypt.HashPassword(model.Password);

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
            else
            {
                return BadRequest(new
                {
                    success = false,
                    message = "User Already Exists!"
                });
            }
        }
        catch (Exception)
        {
            return StatusCode(500, new
            {
                message = "Server Error"
            });
        }
    }

    [HttpPost("Login")]
    public async Task<IActionResult> Login(LoginRequest model)
    {
        try
        {
            var existingUser = await _sqlService.FindUser(model.Email);

            if (existingUser == null)
            {
                return StatusCode(400, new
                {
                    success = false,
                    message = "No User Found"
                });
            }
            else
            {
                var checkPass = BCrypt.Net.BCrypt.Verify(model.Password, existingUser.Password);
                if (checkPass)
                {
                    var token = _tokenService.CreateToken(existingUser.UserId, existingUser.Email, existingUser.Username, 60 * 24, existingUser.Role);
                    return StatusCode(200, new
                    {
                        success = true,
                        message = "Logged In Successfully!",
                        role = existingUser.Role,
                        token
                    });
                }
                else
                {
                    return StatusCode(400, new
                    {
                        success = false,
                        message = "Password Incorrect!"
                    });
                }
            }
        }
        catch (Exception)
        {
            return StatusCode(500, new
            {
                message = "Server Error"
            });
        }
    }

    [HttpDelete("Delete")]
    public async Task<IActionResult> DeleteAccount(LoginRequest model)
    {
        try
        {
            var user = await _sqlService.FindUser(model.Email);

            if (user == null)
            {
                return StatusCode(400, new
                {
                    message = "No User Found"
                });
            }
            
            var passVerify = BCrypt.Net.BCrypt.Verify(model.Password, user.Password);

            if (passVerify)
            {
                await _sqlService.DeleteUser(model.Email);

                return StatusCode(200, new
                {
                    message = "User Account Deleted successfully!"
                });
            }
            else
            {
                return StatusCode(400, new
                {
                    message = "Password incorrect"
                });
            }
        }
        catch (Exception)
        {
            return StatusCode(500, new
            {
                message = "Server Error!"
            });
        }
    }

    [HttpPost("Forgot-pass")]
    public async Task<IActionResult> ForgotPass(EmailRequest model)
    {
        try
        {
            var findUser = await _sqlService.FindUser(model.EMail);

            if (findUser == null)
            {
                return StatusCode(404, new
                {
                    message = "User Not Found!"
                });
            }
            
            var token = _tokenService.CreateToken(findUser.UserId, model.EMail, findUser.Username, 10, findUser.Role);

            string link = $"http://localhost:3000/reset-password?token={token}";
            
            await _mailService.SendEmailAsync(model.EMail, "Forgot Password Link", $"Click here to reset password: {link}", false);

            return Ok(new
            {
                success = true,
                message = "Password reset link is sent to your email",
                token
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return StatusCode(500, new
            {
                message = "Server Error"
            });
        }
    }

    [HttpPost("Change-password")]
    public async Task<IActionResult> ChangePassword(string token, ChangePassRequest updationReq)
    {
        try
        {
            var userId = _tokenService.VerifyTokenAndGetId(token);

            if (updationReq.Password != updationReq.ConfirmPassword)
            {
                return StatusCode(400, new
                {
                    message = "Password does not match"
                });
            }

            var encryptedPass = BCrypt.Net.BCrypt.HashPassword(updationReq.Password);

            var updatePass = await _sqlService.UpdatePass(userId, encryptedPass);

            if (updatePass)
            {
                return Ok(new
                {
                    message = "Password updated Successfully"
                });
            }
            else
            {
                return StatusCode(500, new
                {
                    message = "Some Error during updating password!"
                });
            }
        }
        catch (Exception ex)
        {
            if (ex.Message.Contains("Token has expired"))
            {
                return Unauthorized(new { message = "Password Reset Link Expired!" });
            }
            return StatusCode(500, new
            {
                message = "Server Error"
            });
        }
    }
}
