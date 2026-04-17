using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SoftBridge.Abstraction.IServices.Auth;
using SoftBridge.Shared.Dto_s.Auth.ForgetPssword;
using SoftBridge.Shared.Dto_s.Auth.Sign_In_Up;

namespace SoftBridge.Web.Controllers.Auth
{
    [AllowAnonymous] // to allow anonymous access to all actions in this controller 
    public class AuthController(IAuthService _authService) : AppBaseController
    {
        // 1.  (Register)
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto registerDto)
        {
            var result = await _authService.RegisterAsync(registerDto);

            // make created response with the result and a message
            return Created(result, "User registered successfully");
        }

        // 2.  (Login)
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            var result = await _authService.LoginAsync(loginDto);
            return Success(result, "Logged in successfully");
        }

        // 3.  (Forget Password)
        [HttpPost("forget-password")]
        [EnableRateLimiting("OtpPolicy")]
        public async Task<IActionResult> ForgetPassword(ForgetPasswordDto forgetPasswordDto)
        {
            await _authService.ForgetPasswordAsync(forgetPasswordDto);

            return Success("OTP has been sent to your email successfully");
        }

        // 4.  (Verify OTP)
        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp(VerifyOtpDto verifyOtpDto)
        {
            var isValid = await _authService.VerifyOtpAsync(verifyOtpDto);
            if (!isValid)
                return BadRequestError("Invalid or expired OTP code.");

            return Success(isValid, "OTP verified successfully");
        }

        // 5.  (Reset Password)
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto resetPasswordDto)
        {
            var result = await _authService.ResetPasswordAsync(resetPasswordDto);
            return Success(result, "Password has been reset successfully");
        }
    }
}