using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Cosmetics.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using BCrypt.Net;
using Cosmetics.DTO.User;
using AutoMapper;
using Microsoft.Extensions.Options;
using Cosmetics.Service.OTP;

using Cosmetics.DTO.User.Admin;

namespace ComedicShopAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly ComedicShopDBContext _context;
        private readonly AppSetting _appSettings;
        private readonly IMapper _mapper;
        private readonly IEmailService _emailService;

        public UserController(ComedicShopDBContext context, IOptionsMonitor<AppSetting> optionsMonitor, IMapper mapper, IEmailService emailService)
        {
            _context = context;
            _appSettings = optionsMonitor.CurrentValue;
            _mapper = mapper;
            _emailService = emailService;
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Validate(LoginModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = "Invalid data",
                    Data = ModelState
                });
            }

            var user = _context.Users.SingleOrDefault(p => p.Email == model.Email);
            if (user == null)
            {
                return Ok(new ApiResponse
                {
                    Success = false,
                    Message = "Invalid Username/Password"
                });
            }

            bool passwordValid = false;

            if (user.Password.StartsWith("$2a$"))
            {
                passwordValid = BCrypt.Net.BCrypt.Verify(model.Password, user.Password);
            }
            else
            {
                passwordValid = (model.Password == user.Password);
            }

            if (!passwordValid)
            {
                return Ok(new ApiResponse
                {
                    Success = false,
                    Message = "Invalid Username/Password"
                });
            }

            if (user.Verify != 4)
            {
                return Ok(new ApiResponse
                {
                    Success = false,
                    Message = "Account is not verified. Please verify your account"
                });
            }

            if (user.UserStatus == 1)
            {
                return Ok(new ApiResponse
                {
                    Success = false,
                    Message = "Account is locked. Please contact support for assistance."
                });
            }

            var username = user.Email;
            var firstname = user.FirstName;
            var lastname = user.LastName;
            var id = user.UserId;
            var email = user.Email;
            var phone = user.Phone;
            var role = GetUserRole(user.RoleType);

            if (role == null)
            {
                return Ok(new ApiResponse
                {
                    Success = false,
                    Message = "Invalid Role"
                });
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, id.ToString()),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.MobilePhone, phone),
                new Claim("FirstName", firstname),
                new Claim("LastName", lastname),
                new Claim("Id", id.ToString()),
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
                Data = new { Id = id, FirstName = firstname, LastName = lastname, Email = email, Phone = phone, Role = role, Token = token }
            });
        }

      


        [HttpPost("registerwithotp")]
        public async Task<IActionResult> RegisterWithOtp(SendOtpModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = "Invalid data",
                    Data = ModelState
                });
            }

            if (model.Password != model.ConfirmPassword)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = "Password and confirm password do not match"
                });
            }

            var existingUserByUsername = await _context.Users.SingleOrDefaultAsync(u => u.Email == model.Email);
            if (existingUserByUsername != null)
            {
                return Ok(new ApiResponse
                {
                    Success = false,
                    Message = "Email already exists"
                });
            }


            var existingUserByPhone = await _context.Users.SingleOrDefaultAsync(u => u.Phone == model.Phone);
            if (existingUserByPhone != null)
            {
                return Ok(new ApiResponse
                {
                    Success = false,
                    Message = "Phone number already exists"
                });
            }

            // Generate random OTP
            var otp = new Random().Next(100000, 999999).ToString();

            // Hash OTP before saving to database
            var hashedOtp = BCrypt.Net.BCrypt.HashPassword(otp);

            var user = new User
            {
                Email = model.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(model.Password), // Hashed password
                FirstName = model.FirstName,
                LastName = model.LastName,
                Phone = model.Phone,
                RoleType = 3, // Set RoleType to 3 for User
                Otp = hashedOtp, // Save hashed OTP
                OtpExpiration = DateTime.UtcNow.AddMinutes(3), 
                Verify = 0 // Not verified yet
            };

            _context.Users.Add(user);

            try
            {
                await _emailService.SendEmailAsync(model.Email, "Your OTP Code", $"Your OTP code is {otp}");
                await _context.SaveChangesAsync();

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Registration successful. OTP sent successfully."
                });
            }
            catch (DbUpdateException dbEx)
            {
                // Extract the inner exception details
                var innerException = dbEx.InnerException != null ? dbEx.InnerException.Message : dbEx.Message;
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
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = "Invalid data",
                    Data = ModelState
                });
            }

            var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == model.Email);
            if (user == null)
            {
                return Ok(new ApiResponse
                {
                    Success = false,
                    Message = "Invalid Email/OTP"
                });
            }

            // Compare hashed OTP input with hashed OTP stored in database and check expiration
            if (BCrypt.Net.BCrypt.Verify(model.Otp, user.Otp) && user.OtpExpiration > DateTime.UtcNow)
            {
                user.Verify = 4;
                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "OTP verified successfully"
                });
            }

            return Ok(new ApiResponse
            {
                Success = false,
                Message = "Invalid OTP or OTP has expired"
            });
        }


        [HttpPost("become-affiliate")]
        public async Task<IActionResult> BecomeAffiliate()
        {
            // Lấy thông tin user từ token
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userId, out int parsedUserId))
            {
                return Unauthorized(new ApiResponse { Success = false, Message = "User not authenticated" });
            }

            var user = await _context.Users.FindAsync(parsedUserId);
            if (user == null)
            {
                return NotFound(new ApiResponse { Success = false, Message = "User not found" });
            }

            
            const int AffiliateRole = 2; 
            if (user.RoleType == AffiliateRole)
            {
                return BadRequest(new ApiResponse { Success = false, Message = "You are already an Affiliate" });
            }

    
            user.RoleType = AffiliateRole;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

   
            var newToken = GenerateToken(user);

            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Successfully registered as an Affiliate",
                Data = new { UserId = user.UserId, Role = GetUserRole(user.RoleType), Token = newToken }
            });
        }

        [HttpPost("ChangePassword")]
        public async Task<IActionResult> ChangePassword(ChangePasswordModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = "Invalid data",
                    Data = ModelState
                });
            }

            var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == model.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(model.OldPassword, user.Password))//Hash pass
            {
                return Ok(new ApiResponse
                {
                    Success = false,
                    Message = "Invalid Email or Old Password. Please again"
                });
            }

            user.Password = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Password changed successfully"
            });
        }

        [HttpGet("GetAllUsers")]
        public async Task<IActionResult> GetAllUsers()
        {
            if (!IsAdmin(User))
            {
                return Unauthorized(new ApiResponse
                {
                    Success = false,
                    Message = "Unauthorized"
                });
            }

            var users = await _context.Users.ToListAsync();

            // Map each user to UserAdminDTO
            var userDtoList = new List<UserAdminDTO>();
            foreach (var user in users)
            {
                var userDto = _mapper.Map<UserAdminDTO>(user);

                // Convert RoleType from int to string (assuming GetUserRole is a method you have elsewhere)
                userDto.RoleType = user.RoleType;

                userDtoList.Add(userDto);
            }

            return Ok(new ApiResponse
            {
                Success = true,
                Data = userDtoList
            });
        }
        // GetById
        //
        [HttpGet("GetUserById/{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            if (!IsAdmin(User))
            {
                return Unauthorized(new ApiResponse
                {
                    Success = false,
                    Message = "Unauthorized"
                });
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound(new ApiResponse
                {
                    Success = false,
                    Message = "User not found"
                });
            }

            // Map the user to UserAdminDTO
            var userDto = _mapper.Map<UserAdminDTO>(user);

            // Convert the RoleType from int to string
            userDto.RoleType = user.RoleType;

            return Ok(new ApiResponse
            {
                Success = true,
                Data = userDto
            });
        }



        [HttpGet("GetCurrentUser")]
        [Authorize]
        //User and Admin
        public async Task<IActionResult> GetCurrentUser()
        {
            // Log claims for debugging
            foreach (var claim in User.Claims)
            {
                Console.WriteLine($"Claim Type: {claim.Type}, Claim Value: {claim.Value}");
            }

            // Get the user ID from the claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized(new ApiResponse
                {
                    Success = false,
                    Message = "User ID claim not found"
                });
            }

            // Parse the user ID
            if (!int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized(new ApiResponse
                {
                    Success = false,
                    Message = "Invalid user ID"
                });
            }

            // Retrieve the user from the database
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound(new ApiResponse
                {
                    Success = false,
                    Message = "User not found"
                });
            }

            // Map the user to a DTO
            var userDTO = _mapper.Map<UserDTO>(user);

            // Return the user details
            return Ok(new ApiResponse
            {
                Success = true,
                Message = "User found",
                Data = userDTO
            });
        }

        //Edit Account (User)
        [HttpPut("EditSelf")]
        [Authorize]
        public async Task<IActionResult> EditSelf(EditSelfModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = "Invalid data"
                });
            }

            // Get the ID of the user making the request
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized(new ApiResponse
                {
                    Success = false,
                    Message = "User ID claim not found"
                });
            }

            if (!int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized(new ApiResponse
                {
                    Success = false,
                    Message = "Invalid user ID"
                });
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound(new ApiResponse
                {
                    Success = false,
                    Message = "User not found"
                });
            }
            var existingUserByPhone = await _context.Users.SingleOrDefaultAsync(u => u.Phone == model.Phone && u.UserId != userId);
            if (existingUserByPhone != null)
            {
                return Ok(new ApiResponse
                {
                    Success = false,
                    Message = "Phone number already exists"
                });
            }

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Phone = model.Phone;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return Ok(new ApiResponse
            {
                Success = true,
                Message = "User updated successfully"
            });
        }


        [HttpPut("EditUserStatusAndRole/{id}")]
        [Authorize]
        public async Task<IActionResult> EditUserStatusAndRole(int id, EditUserStatusAndRoleModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = "Invalid data"
                });
            }

            // Kiểm tra quyền Admin
            if (!IsAdmin(User))
            {
                return Unauthorized(new ApiResponse
                {
                    Success = false,
                    Message = "Access denied. Admins only."
                });
            }

            // Tìm người dùng theo ID
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound(new ApiResponse
                {
                    Success = false,
                    Message = "User not found"
                });
            }

            // Cập nhật UserStatus nếu có
            if (model.UserStatus.HasValue)
            {
                if (model.UserStatus < 0 || model.UserStatus > 1)
                {
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "Invalid UserStatus"
                    });
                }
                user.UserStatus = model.UserStatus.Value;
            }

            // Cập nhật RoleType nếu có
            if (model.RoleType.HasValue)
            {
                if (model.RoleType < 0 || model.RoleType > 5)
                {
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "Invalid RoleType"
                    });
                }
                var roleName = GetUserRole(model.RoleType.Value);
                if (string.IsNullOrEmpty(roleName))
                {
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "Invalid RoleType"
                    });
                }
                user.RoleType = model.RoleType.Value;
            }

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return Ok(new ApiResponse
            {
                Success = true,
                Message = "User status and role updated successfully"
            });
        }


         [HttpPost("forgotpassword")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = "Invalid data",
                    Data = ModelState
                });
            }

            var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == model.Email);
            if (user == null)
            {
                return Ok(new ApiResponse
                {
                    Success = false,
                    Message = "Invalid Email"
                });
            }

            // Generate password reset token
            var token = Guid.NewGuid().ToString();
            user.RefreshToken = BCrypt.Net.BCrypt.HashPassword(token);
            user.TokenExpiry = DateTime.UtcNow.AddHours(1);

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            var resetLink = $"http://localhost:3000/newPass?token={token}";

            try
            {
                await _emailService.SendEmailAsync(user.Email, "Reset Password", $"Click the link to reset your password: {resetLink}");
                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Password reset link has been sent to your email."
                });
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
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = "Invalid data",
                    Data = ModelState
                });
            }

            if (model.NewPassword != model.ConfirmPassword)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = "Password and confirm password do not match"
                });
            }

            var user = await _context.Users
                .Where(u => u.TokenExpiry > DateTime.UtcNow)
                .ToListAsync();

            var matchedUser = user.SingleOrDefault(u => BCrypt.Net.BCrypt.Verify(model.Token, u.RefreshToken));

            if (matchedUser == null)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = "Invalid or expired token"
                });
            }

            matchedUser.Password = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
            matchedUser.RefreshToken = null;
            matchedUser.TokenExpiry = null;

            _context.Users.Update(matchedUser);
            await _context.SaveChangesAsync();

            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Password has been reset successfully"
            });
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
            return user.Claims.Any(c => c.Type == ClaimTypes.Role && (c.Value == "Administrator"));
        }
        private string GetUserRole(int roleType)
        {
            switch (roleType)
            {
                case 0:
                    return "Administrator";
                case 1:
                    return "Manager";
                case 2:
                    return "Affiliates";
                case 3:
                    return "Customers";
                case 4:
                    return "Sales Staff";
                case 5:
                    return "Shipper Staff";

                default:

                    return null;
            }
        }
    }
}

