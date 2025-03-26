using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Cosmetics.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Cosmetics.DTO.User;
using AutoMapper;
using Cosmetics.Service.OTP;
using Cosmetics.DTO.User.Admin;
using Cosmetics.Repositories.UnitOfWork;
using Cosmetics.DTO.Affiliate;
//using Cosmetics.Service.Affiliate;

namespace ComedicShopAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly AppSetting _appSettings;
        private readonly IMapper _mapper;
        private readonly IEmailService _emailService;
        //private readonly IAffiliateService _affiliateService;
        private readonly ComedicShopDBContext _context;


        public UserController(ComedicShopDBContext context,IUnitOfWork unitOfWork, IOptionsMonitor<AppSetting> optionsMonitor, IMapper mapper, IEmailService emailService/* IAffiliateService affiliateService*/)
        {
            _unitOfWork = unitOfWork;
            _appSettings = optionsMonitor.CurrentValue;
            _mapper = mapper;
            _emailService = emailService;
            //_affiliateService = affiliateService;
            _context = context;
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Validate(LoginModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse { Success = false, Message = "Invalid data", Data = ModelState });
            }

            var user = await _unitOfWork.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
            if (user == null)
            {
                return Ok(new ApiResponse { Success = false, Message = "Invalid Username/Password" });
            }

            bool passwordValid = user.Password.StartsWith("$2a$")
                ? BCrypt.Net.BCrypt.Verify(model.Password, user.Password)
                : model.Password == user.Password;

            if (!passwordValid)
            {
                return Ok(new ApiResponse { Success = false, Message = "Invalid Username/Password" });
            }

            if (user.Verify != 4)
            {
                return Ok(new ApiResponse { Success = false, Message = "Account is not verified. Please verify your account" });
            }

            if (user.UserStatus == 1)
            {
                return Ok(new ApiResponse { Success = false, Message = "Account is locked. Please contact support for assistance." });
            }

            var role = GetUserRole(user.RoleType);
            if (role == null)
            {
                return Ok(new ApiResponse { Success = false, Message = "Invalid Role" });
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.MobilePhone, user.Phone),
                new Claim("FirstName", user.FirstName),
                new Claim("LastName", user.LastName),
                new Claim("Id", user.UserId.ToString()),
                new Claim(ClaimTypes.Role, role)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(3)
            };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

            var token = GenerateToken(user);

            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Authentication successful",
                Data = new { Id = user.UserId, FirstName = user.FirstName, LastName = user.LastName, Email = user.Email, Phone = user.Phone, Role = role, Token = token }
            });
        }

        [HttpPost("registerwithotp")]
        public async Task<IActionResult> RegisterWithOtp(SendOtpModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse { Success = false, Message = "Invalid data", Data = ModelState });
            }

            if (model.Password != model.ConfirmPassword)
            {
                return BadRequest(new ApiResponse { Success = false, Message = "Password and confirm password do not match" });
            }

            if (await _unitOfWork.Users.AnyAsync(u => u.Email == model.Email))
            {
                return Ok(new ApiResponse { Success = false, Message = "Email already exists" });
            }

            if (await _unitOfWork.Users.AnyAsync(u => u.Phone == model.Phone))
            {
                return Ok(new ApiResponse { Success = false, Message = "Phone number already exists" });
            }

            var otp = new Random().Next(100000, 999999).ToString();
            var hashedOtp = BCrypt.Net.BCrypt.HashPassword(otp);

            var user = new User
            {
                Email = model.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(model.Password),
                FirstName = model.FirstName,
                LastName = model.LastName,
                Phone = model.Phone,
                RoleType = 3, // Customer
                Otp = hashedOtp,
                OtpExpiration = DateTime.UtcNow.AddMinutes(3),
                Verify = 0
            };

            await _unitOfWork.Users.AddAsync(user);

            try
            {
                await _emailService.SendEmailAsync(model.Email, "Your OTP Code", $"Your OTP code is {otp}");
                await _unitOfWork.CompleteAsync();
                return Ok(new ApiResponse { Success = true, Message = "Registration successful. OTP sent successfully." });
            }
            catch (DbUpdateException dbEx)
            {
                var innerException = dbEx.InnerException?.Message ?? dbEx.Message;
                return StatusCode(500, new ApiResponse { Success = false, Message = $"Failed to send OTP: {innerException}" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse { Success = false, Message = $"An unexpected error occurred: {ex.Message}" });
            }
        }
        [HttpPost("verifyotp")]
        public async Task<IActionResult> VerifyOtp(VerifyOtpModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse { Success = false, Message = "Invalid data", Data = ModelState });
            }

            var user = await _unitOfWork.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
            if (user == null)
            {
                return Ok(new ApiResponse { Success = false, Message = "Invalid Email/OTP" });
            }

            if (BCrypt.Net.BCrypt.Verify(model.Otp, user.Otp) && user.OtpExpiration > DateTime.UtcNow)
            {
                user.Verify = 4;
                _unitOfWork.Users.UpdateAsync(user);
                await _unitOfWork.CompleteAsync();
                return Ok(new ApiResponse { Success = true, Message = "OTP verified successfully" });
            }

            return Ok(new ApiResponse { Success = false, Message = "Invalid OTP or OTP has expired" });
        }

        



        [HttpPost("ChangePassword")]
        public async Task<IActionResult> ChangePassword(ChangePasswordModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse { Success = false, Message = "Invalid data", Data = ModelState });
            }

            var user = await _unitOfWork.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(model.OldPassword, user.Password))
            {
                return Ok(new ApiResponse { Success = false, Message = "Invalid Email or Old Password. Please try again" });
            }

            user.Password = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
            _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.CompleteAsync();

            return Ok(new ApiResponse { Success = true, Message = "Password changed successfully" });
        }

        [HttpGet("GetAllUsers")]
        public async Task<IActionResult> GetAllUsers()
        {
            if (!IsAdmin(User))
            {
                return Unauthorized(new ApiResponse { Success = false, Message = "Unauthorized" });
            }

            var users = await _unitOfWork.Users.GetAllAsync();
            var userDtoList = users.Select(user => _mapper.Map<UserAdminDTO>(user))
                                   .Select(dto => { dto.RoleName = GetUserRole(dto.RoleType); return dto; })
                                   .ToList();

            return Ok(new ApiResponse { Success = true, Data = userDtoList });
        }

        [HttpGet("GetUserById/{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            if (!IsAdmin(User))
            {
                return Unauthorized(new ApiResponse { Success = false, Message = "Unauthorized" });
            }

            var user = await _unitOfWork.Users.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound(new ApiResponse { Success = false, Message = "User not found" });
            }

            var userDto = _mapper.Map<UserAdminDTO>(user);
            userDto.RoleName = GetUserRole(user.RoleType);

            return Ok(new ApiResponse { Success = true, Data = userDto });
        }

        [HttpGet("GetCurrentUser")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized(new ApiResponse { Success = false, Message = "Invalid user ID" });
            }

            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new ApiResponse { Success = false, Message = "User not found" });
            }

            var userDTO = _mapper.Map<UserDTO>(user);
            return Ok(new ApiResponse { Success = true, Message = "User found", Data = userDTO });
        }

        [HttpPut("EditSelf")]
        [Authorize]
        public async Task<IActionResult> EditSelf(EditSelfModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse { Success = false, Message = "Invalid data" });
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized(new ApiResponse { Success = false, Message = "Invalid user ID" });
            }

            // Kiểm tra vai trò từ Claims
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            if (userRole == "Affiliates") // Giả sử vai trò "Affiliate" được lưu trong Claims
            {
                return BadRequest(new ApiResponse { Success = false, Message = "Affiliates are not allowed to edit their profile." });
            }

            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new ApiResponse { Success = false, Message = "User not found" });
            }

            if (await _unitOfWork.Users.AnyAsync(u => u.Phone == model.Phone && u.UserId != userId))
            {
                return Ok(new ApiResponse { Success = false, Message = "Phone number already exists" });
            }

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Phone = model.Phone;

            _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.CompleteAsync();

            return Ok(new ApiResponse { Success = true, Message = "User updated successfully" });
        }

        [HttpPut("EditUserStatusAndRole/{id}")]
        [Authorize]
        public async Task<IActionResult> EditUserStatusAndRole(int id, EditUserStatusAndRoleModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse { Success = false, Message = "Invalid data" });
            }

            if (!IsAdmin(User))
            {
                return Unauthorized(new ApiResponse { Success = false, Message = "Access denied. Admins only." });
            }

            var user = await _unitOfWork.Users.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound(new ApiResponse { Success = false, Message = "User not found" });
            }

            if (model.UserStatus.HasValue)
            {
                if (model.UserStatus < 0 || model.UserStatus > 1)
                {
                    return BadRequest(new ApiResponse { Success = false, Message = "Invalid UserStatus" });
                }
                user.UserStatus = model.UserStatus.Value;
            }

            if (model.RoleType.HasValue)
            {
                if (model.RoleType < 0 || model.RoleType > 5)
                {
                    return BadRequest(new ApiResponse { Success = false, Message = "Invalid RoleType" });
                }
                var roleName = GetUserRole(model.RoleType.Value);
                if (string.IsNullOrEmpty(roleName))
                {
                    return BadRequest(new ApiResponse { Success = false, Message = "Invalid RoleType" });
                }
                user.RoleType = model.RoleType.Value;
            }

            _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.CompleteAsync();

            return Ok(new ApiResponse { Success = true, Message = "User status and role updated successfully" });
        }

        [HttpPost("forgotpassword")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse { Success = false, Message = "Invalid data", Data = ModelState });
            }

            var user = await _unitOfWork.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
            if (user == null)
            {
                return Ok(new ApiResponse { Success = false, Message = "Invalid Email" });
            }

            var token = Guid.NewGuid().ToString();
            user.RefreshToken = BCrypt.Net.BCrypt.HashPassword(token);
            user.TokenExpiry = DateTime.UtcNow.AddHours(1);

            _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.CompleteAsync();

            var resetLink = $"http://localhost:3000/newPass?token={token}";
            try
            {
                await _emailService.SendEmailAsync(user.Email, "Reset Password", $"Click the link to reset your password: {resetLink}");
                return Ok(new ApiResponse { Success = true, Message = "Password reset link has been sent to your email." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse { Success = false, Message = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpPost("newPass")]
        public async Task<IActionResult> ResetPassword(SetNewPasswordModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse { Success = false, Message = "Invalid data", Data = ModelState });
            }

            if (model.NewPassword != model.ConfirmPassword)
            {
                return BadRequest(new ApiResponse { Success = false, Message = "Password and confirm password do not match" });
            }

            var users = await _unitOfWork.Users.GetAllAsync(u => u.TokenExpiry > DateTime.UtcNow);
            var matchedUser = users.FirstOrDefault(u => BCrypt.Net.BCrypt.Verify(model.Token, u.RefreshToken));

            if (matchedUser == null)
            {
                return BadRequest(new ApiResponse { Success = false, Message = "Invalid or expired token" });
            }

            matchedUser.Password = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
            matchedUser.RefreshToken = null;
            matchedUser.TokenExpiry = null;

            _unitOfWork.Users.UpdateAsync(matchedUser);
            await _unitOfWork.CompleteAsync();

            return Ok(new ApiResponse { Success = true, Message = "Password has been reset successfully" });
        }

        private string GenerateToken(User user)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();
            var secretKeyBytes = Encoding.UTF8.GetBytes(_appSettings.SecretKey);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.MobilePhone, user.Phone),
                    new Claim("FirstName", user.FirstName),
                    new Claim("LastName", user.LastName),
                    new Claim(ClaimTypes.Role, GetUserRole(user.RoleType))
                }),
                Expires = DateTime.UtcNow.AddHours(3),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(secretKeyBytes), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = jwtTokenHandler.CreateToken(tokenDescriptor);
            return jwtTokenHandler.WriteToken(token);
        }

        private bool IsAdmin(ClaimsPrincipal user)
        {
            return user.Claims.Any(c => c.Type == ClaimTypes.Role && c.Value == "Administrator");
        }

        private string GetUserRole(int roleType)
        {
            switch (roleType)
            {
                case 0: return "Administrator";
                case 1: return "Manager";
                case 2: return "Affiliates";
                case 3: return "Customers";
                case 4: return "Sales Staff";
                case 5: return "Shipper Staff";
                default: return null;
            }
        }
    }
}