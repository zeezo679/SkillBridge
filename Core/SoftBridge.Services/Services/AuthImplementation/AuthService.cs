using AutoMapper;
using Microsoft.AspNetCore.Identity;
using SoftBridge.Abstraction.IServices.Auth;
using SoftBridge.Abstraction.IServicesContract.Notification;
using SoftBridge.Abstraction.IServicesContract.Token;
using SoftBridge.Domain.Contracts.UnitOfWorkPattern;
using SoftBridge.Domain.Exceptions;
using SoftBridge.Domain.Models.AccountAggregates;
using SoftBridge.Domain.Models.EnumHelper;
using SoftBridge.Domain.Models.User;
using SoftBridge.Services.Services.NotificationImplementation;
using SoftBridge.Shared.Common.Dto.Notification;
using SoftBridge.Shared.Dto_s.Auth.ForgetPssword;
using SoftBridge.Shared.Dto_s.Auth.Sign_In_Up;
using SoftBridge.Shared.Dto_s.Token;

namespace SoftBridge.Services.Services.AuthImplementation
{
    public class AuthService(UserManager<ApplicationUser> _userManager, IUnitOfWork _unitOfWork, IMapper _mapper, ITokenService _tokenService ,INotificationService notificationService)
           : IAuthService
    {
        public async Task<AuthModelDto> RegisterAsync(RegisterDto registerDto)
        {
            // 1 - Check if the user already exists
            var user = await _userManager.FindByEmailAsync(registerDto.Email);
            if (user != null)
                throw new BadRequestExceptionCustome("User with this email already exists.");

            // 2 - Mapping and Create User
            var userForDB = _mapper.Map<ApplicationUser>(registerDto);
            var result = await _userManager.CreateAsync(userForDB, registerDto.Password);

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description);
                throw new BadRequestExceptionCustome("User registration failed", errors);
            }

            // 3 - Assign Role and Create Profile (Using Helper)
            await AssignRoleAndCreateProfileAsync(userForDB, registerDto.UserType);

            // 4 - Generate Token and Return (Using Helper)
            return await GenerateAuthModelAsync(userForDB);
        }
        public async Task<AuthModelDto> LoginAsync(LoginDto loginDto)
        {
            // 1 - Get user
            var user = await _userManager.FindByEmailAsync(loginDto.Email);
            if (user == null)
                throw new UnauthorizedExceptionCusotme();

            // 2 - Check password
            var result = await _userManager.CheckPasswordAsync(user, loginDto.Password);
            if (!result)
                throw new UnauthorizedExceptionCusotme();

            // 3 - Generate Token and Return (Using Helper)
            return await GenerateAuthModelAsync(user);
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
            var message = new NotificationContentDto 
            {
                UserId = user.Id,
                Email = user.Email,
                Subject = "SoftBridge - رمز استعادة كلمة المرور",
                Body = $"كود استعادة كلمة المرور الخاص بك هو: {otp}\nهذا الكود صالح لمدة 5 دقيقة ولا تشاركه مع أحد."
            };

            await notificationService.SendNotificationAsync(message, NotificationType.Email);
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
            var user = await _userManager.FindByEmailAsync(resetPasswordDto.Email);
            if (user == null)
                throw new UnauthorizedExceptionCusotme();

            var result = await _userManager.ResetPasswordAsync(user, resetPasswordDto.Otp, resetPasswordDto.NewPassword);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description);
                throw new BadRequestExceptionCustome("Password reset failed.", errors);
            }

            // Generate Token and Return (Using Helper)
            return await GenerateAuthModelAsync(user);
        }

        private async Task AssignRoleAndCreateProfileAsync(ApplicationUser user, UserType userType)
        {
            await _userManager.AddToRoleAsync(user, userType.ToString());

            if (userType == UserType.Provider)
            {
                var providerRepo = _unitOfWork.GetRepository<SProvider, Guid>();
                await providerRepo.AddAsync(new SProvider
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    Status = ProviderAccountStatus.Pending
                });
            }
            else
            {
                var clientRepo = _unitOfWork.GetRepository<Client, Guid>();
                await clientRepo.AddAsync(new Client
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id
                });
            }

            await _unitOfWork.SaveChangesAsync();
        }

        private async Task<AuthModelDto> GenerateAuthModelAsync(ApplicationUser user)
        {
            var userRoles = await _userManager.GetRolesAsync(user);

            var tokenRequest = new TokenRequestDto
            {
                UserId = user.Id,
                Email = user.Email!,
                UserName = user.FullName,
                Roles = userRoles
            };

            var tokenResponse = await _tokenService.CreateTokenAsync(tokenRequest);
            if (string.IsNullOrEmpty(tokenResponse.Token))
                throw new BadRequestExceptionCustome("Token generation failed.");

            return new AuthModelDto
            {
                Token = tokenResponse.Token,
                IsAuthenticated = true,
                ExpireOn = tokenResponse.ExpireOn,
                Email = user.Email!,
                Name = user.FullName,
                Roles = userRoles
            };
        }
    }
}