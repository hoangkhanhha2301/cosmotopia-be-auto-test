using AutoMapper;
using ComedicShopAPI.Controllers;
using Cosmetics.DTO.User.Admin;
using Cosmetics.DTO.User;
using Cosmetics.Models;
using Cosmetics.Repositories.UnitOfWork;
using Cosmetics.Service.OTP;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Cosmetics.Tests
{
    public class UserAdminControllerTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<IOptionsMonitor<AppSetting>> _mockAppSetting;
        private readonly Mock<IEmailService> _mockEmailService;
        private readonly UserController _controller;

        public UserAdminControllerTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockMapper = new Mock<IMapper>();
            _mockAppSetting = new Mock<IOptionsMonitor<AppSetting>>();
            _mockAppSetting.Setup(x => x.CurrentValue).Returns(new AppSetting { SecretKey = "SecretKey" });
            _mockEmailService = new Mock<IEmailService>();

            _controller = new UserController(null, _mockUnitOfWork.Object, _mockAppSetting.Object, _mockMapper.Object, _mockEmailService.Object);
        }

        private void SetAdminUserContext()
        {
            var adminClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Role, "Administrator")
            };
            var identity = new ClaimsIdentity(adminClaims);
            var principal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };
        }

        private void SetNonAdminUserContext()
        {
            var claims = new List<Claim> { new Claim(ClaimTypes.Role, "Customer") };
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };
        }

        [Fact]
        public async Task GetAllUsers_ReturnsUnauthorized_WhenNotAdmin()
        {
            SetNonAdminUserContext();

            var result = await _controller.GetAllUsers();

            var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);
            var response = Assert.IsType<ApiResponse>(unauthorized.Value);
            Assert.False(response.Success);
            Assert.Equal("Unauthorized", response.Message);
        }

        [Fact]
        public async Task GetAllUsers_ReturnsUserList_WhenAdmin()
        {
            SetAdminUserContext();

            var users = new List<User> { new User { UserId = 1, FirstName = "John", RoleType = 0 } };
            var mapped = new List<UserAdminDTO> { new UserAdminDTO { UserId = 1, FirstName = "John", RoleType = 0, RoleName = "Administrator" } };

            _mockUnitOfWork.Setup(x => x.Users.GetAllAsync()).ReturnsAsync(users);
            _mockMapper.Setup(x => x.Map<UserAdminDTO>(It.IsAny<User>())).Returns((User u) => new UserAdminDTO
            {
                UserId = u.UserId,
                FirstName = u.FirstName,
                RoleType = u.RoleType,
                RoleName = "Administrator"
            });

            var result = await _controller.GetAllUsers();

            var ok = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ApiResponse>(ok.Value);
            Assert.True(response.Success);
            var data = Assert.IsAssignableFrom<List<UserAdminDTO>>(response.Data);
            Assert.Single(data);
            Assert.Equal("John", data[0].FirstName);
        }

        [Fact]
        public async Task GetUserById_ReturnsUnauthorized_WhenNotAdmin()
        {
            SetNonAdminUserContext();
            var result = await _controller.GetUserById(1);

            var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);
            var response = Assert.IsType<ApiResponse>(unauthorized.Value);
            Assert.False(response.Success);
            Assert.Equal("Unauthorized", response.Message);
        }

        [Fact]
        public async Task GetUserById_ReturnsNotFound_WhenUserMissing()
        {
            SetAdminUserContext();
            _mockUnitOfWork.Setup(x => x.Users.GetByIdAsync(1)).ReturnsAsync((User)null);

            var result = await _controller.GetUserById(1);

            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            var response = Assert.IsType<ApiResponse>(notFound.Value);
            Assert.False(response.Success);
            Assert.Equal("User not found", response.Message);
        }

        [Fact]
        public async Task GetUserById_ReturnsUser_WhenExists()
        {
            SetAdminUserContext();

            var user = new User { UserId = 1, FirstName = "Admin", RoleType = 0 };
            var dto = new UserAdminDTO { UserId = 1, FirstName = "Admin", RoleType = 0, RoleName = "Administrator" };

            _mockUnitOfWork.Setup(x => x.Users.GetByIdAsync(1)).ReturnsAsync(user);
            _mockMapper.Setup(x => x.Map<UserAdminDTO>(user)).Returns(dto);

            var result = await _controller.GetUserById(1);

            var ok = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ApiResponse>(ok.Value);
            Assert.True(response.Success);
            Assert.Equal("Admin", ((UserAdminDTO)response.Data).FirstName);
        }
    }
}
