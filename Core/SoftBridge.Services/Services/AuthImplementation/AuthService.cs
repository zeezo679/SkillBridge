using AutoMapper;
using Microsoft.AspNetCore.Identity;
using SoftBridge.Abstraction.IServices.Auth;
using SoftBridge.Abstraction.IServicesContract.Notification;
using SoftBridge.Abstraction.IServicesContract.Token;
using SoftBridge.Domain.Exceptions;
using SoftBridge.Domain.Models.EnumHelper;
using SoftBridge.Domain.Models.User;
using SoftBridge.Shared.Dto_s.Auth.ForgetPssword;
using SoftBridge.Shared.Dto_s.Auth.Sign_In_Up;
using SoftBridge.Shared.Dto_s.Token;

namespace SoftBridge.Services.Services.AuthImplementation
{
    public class AuthService(UserManager<ApplicationUser> _userManager, IMapper _mapper, ITokenService _tokenService)
           : IAuthService
    {
        public async Task<AuthModelDto> RegisterAsync(RegisterDto registerDto)
        {
            // 1 - Check if the user already exists
            var user = await _userManager.FindByEmailAsync(registerDto.Email);
            if (user != null)
                throw new BadRequestExceptionCustome("User with this email already exists.");

            // 2 - mapping the data from the DTO to the ApplicationUser model
            var userForDB = _mapper.Map<ApplicationUser>(registerDto);

            // 3 - Create the user in the database
            var result = await _userManager.CreateAsync(userForDB, registerDto.Password);

            // 4 - Check if the user creation was successful
            if (!result.Succeeded)
            {
                // If user creation failed, throw an exception with the error details
                var errors = result.Errors.Select(e => e.Description);
                throw new BadRequestExceptionCustome("User registration failed", errors);
            }

            // add the user to role
            if (registerDto.UserType == UserType.Provider)
                await _userManager.AddToRoleAsync(userForDB, UserType.Provider.ToString());
            else
                await _userManager.AddToRoleAsync(userForDB, UserType.Client.ToString());

            var userRoles = await _userManager.GetRolesAsync(userForDB);
            // 5 - Generate Token
            var tokenRequest = new TokenRequestDto
            {
                UserId = userForDB.Id,
                Email = userForDB!.Email!,
                UserName = userForDB.FullName,
                Roles = userRoles
            };
            var tokenResponse = await _tokenService.CreateTokenAsync(tokenRequest);
            if (string.IsNullOrEmpty(tokenResponse.Token))
                throw new BadRequestExceptionCustome("Token generation failed.");

            // 6 - Return the authentication model with the token and user details
            return new AuthModelDto
            {
                Token = tokenResponse.Token,
                IsAuthenticated = true,
                ExpireOn = tokenResponse.ExpireOn,
                Email = userForDB.Email!,
                Name = userForDB.FullName,
                Roles = userRoles
            };


        }
        public async Task<AuthModelDto> LoginAsync(LoginDto loginDto)
        {

            // 1 - Get the user from the database
            var user = await _userManager.FindByEmailAsync(loginDto.Email);
            // 2 - Check if the user exists
            if (user == null)
                throw new UnauthorizedExceptionCusotme();

            // 3 - Check if the password is correct
            var result = await _userManager.CheckPasswordAsync(user!, loginDto.Password);
            if (!result)
                throw new UnauthorizedExceptionCusotme();

            // 4 - Generate Token
            var userRoles = await _userManager.GetRolesAsync(user!);
            var tokenRequest = new TokenRequestDto
            {
                UserId = user!.Id!,
                Email = user.Email!,
                UserName = user.FullName,
                Roles = userRoles
            };
            var tokenResp = await _tokenService.CreateTokenAsync(tokenRequest);
            if (string.IsNullOrEmpty(tokenResp.Token))
                throw new BadRequestExceptionCustome("Token generation failed.");

            // 5 - Return the authentication model with the token and user details
            return new AuthModelDto
            {
                Token = tokenResp.Token,
                IsAuthenticated = true,
                ExpireOn = tokenResp.ExpireOn,
                Email = user.Email!,
                Name = user.FullName,
                Roles = userRoles
            };
        }
        public async Task ForgetPasswordAsync(ForgetPasswordDto forgetPasswordDto)
        {
            // 1 - Get the user from the database
            var user = await _userManager.FindByEmailAsync(forgetPasswordDto.Email);
            if (user == null)
                throw new UnauthorizedExceptionCusotme();

            // 2 - Generate OTP by => UserId + CurrentTime
            // // it changed in table users in col callled :
            // SecurityStamp his content changed every time we generate new OTP and we use it to verify the OTP later
            var otp = await _userManager.GeneratePasswordResetTokenAsync(user);

            // 3 - Send OTP to user's email (this is a placeholder, you should implement actual email sending logic)
            #region Email service 
            // 3 - Send OTP 
            //var message = new MessageDto
            //{
            //    To = user.Email!,
            //    Subject = "Password Reset OTP",
            //    Body = $"Your OTP code is: {otp}. It is valid for 2 min."
            //};

            //await _notificationService.SendNotificationAsync(message, NotificationType.Email);
            #endregion
        }
        public async Task<bool> VerifyOtpAsync(VerifyOtpDto verifyOtpDto)
        {
            // 1 - get the user from the database
            var user = await _userManager.FindByEmailAsync(verifyOtpDto.Email);
            if (user == null)
                throw new UnauthorizedExceptionCusotme();

            //2 - Verify the OTP
            var isvalid = await _userManager.VerifyUserTokenAsync(
                user,
                _userManager.Options.Tokens.PasswordResetTokenProvider,
                UserManager<ApplicationUser>.ResetPasswordTokenPurpose, // not _usermanager because the field is static 
                verifyOtpDto.Otp);

            return isvalid;
        }
        public async Task<AuthModelDto> ResetPasswordAsync(ResetPasswordDto resetPasswordDto)
        {
            // 1 - get the user from the database
            var user = await _userManager.FindByEmailAsync(resetPasswordDto.Email);
            if (user == null)
                throw new UnauthorizedExceptionCusotme();

            // 2 - change the user password by otp 
            var result = await _userManager.ResetPasswordAsync(user, resetPasswordDto.Otp, resetPasswordDto.NewPassword);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description);
                throw new BadRequestExceptionCustome("Password reset failed.", errors);
            }

            // 3 - generate new token to kept the user login after change password 
            var userRoles = await _userManager.GetRolesAsync(user);
            var tokenRequest = new TokenRequestDto
            {
                UserId = user.Id,
                Email = user.Email!,
                UserName = user.FullName,
                Roles = userRoles,
            };
            var tokenRespo = await _tokenService.CreateTokenAsync(tokenRequest);

            return new AuthModelDto
            {
                Token = tokenRespo.Token,
                IsAuthenticated = true,
                ExpireOn = tokenRespo.ExpireOn,
                Email = user.Email!,
                Name = user.FullName,
                Roles = userRoles
            };
        }
    }
}