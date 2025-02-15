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

namespace ComedicShopAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly ComedicShopDBContext _context;
        private readonly AppSetting _appSettings;
        private readonly IMapper _mapper;

        public UserController(ComedicShopDBContext context, IOptionsMonitor<AppSetting> optionsMonitor, IMapper mapper)
        {
            _context = context;
            _appSettings = optionsMonitor.CurrentValue;
            _mapper = mapper;
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
                default:
                    return null;
            }
        }
    }
}

