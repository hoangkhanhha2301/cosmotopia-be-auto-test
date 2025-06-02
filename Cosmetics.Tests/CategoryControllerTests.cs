using AutoMapper;
using Cosmetics.Controllers;
using Cosmetics.DTO.Category;
using Cosmetics.DTO.User;
using Cosmetics.Models;
using Cosmetics.Repositories.UnitOfWork;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Cosmetics.Tests
{
    public class CategoryControllerTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IMapper> _mockMapper;
        private readonly CategoryController _controller;

        public CategoryControllerTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockMapper = new Mock<IMapper>();
            _controller = new CategoryController(_mockUnitOfWork.Object, _mockMapper.Object);
        }

        [Fact]
        public async Task GetCategories_ReturnsPaginatedList()
        {
            var categories = new List<Category>
            {
                new Category { CategoryId = Guid.NewGuid(), Name = "Cat A" },
                new Category { CategoryId = Guid.NewGuid(), Name = "Cat B" }
            };
            _mockUnitOfWork.Setup(x => x.Categories.GetAllAsync()).ReturnsAsync(categories);
            _mockMapper.Setup(m => m.Map<List<CategoryDTO>>(It.IsAny<List<Category>>()))
                .Returns(categories.Select(c => new CategoryDTO { Name = c.Name }).ToList());

            var result = await _controller.GetCategories();

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task GetById_ReturnsNotFound_WhenCategoryDoesNotExist()
        {
            _mockUnitOfWork.Setup(x => x.Categories.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Category)null);

            var result = await _controller.GetById(Guid.NewGuid());

            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsAssignableFrom<ApiResponse>(okResult.Value);
            Assert.Equal("Category not found!", response.Message);
            Assert.False(response.Success);
        }

        [Fact]
        public async Task Create_ReturnsSuccess_WhenCategoryIsValid()
        {
            var dto = new CategoryCreateDTO { Name = "New Cat", Description = "Desc" };
            _mockUnitOfWork.Setup(x => x.Categories.categoryNameExist(dto.Name)).ReturnsAsync(false);
            _mockMapper.Setup(m => m.Map<CategoryDTO>(It.IsAny<Category>())).Returns(new CategoryDTO { Name = dto.Name });

            var result = await _controller.Create(dto);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsAssignableFrom<ApiResponse>(okResult.Value);
            Assert.True(response.Success);
            Assert.Equal("Created Category Successfully.", response.Message);
        }

        [Fact]
        public async Task DeleteById_ReturnsForbidden_WhenCategoryHasProducts()
        {
            var id = Guid.NewGuid();
            _mockUnitOfWork.Setup(x => x.Categories.categoryHasProducts(id)).ReturnsAsync(true);

            var result = await _controller.DeleteById(id);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsAssignableFrom<ApiResponse>(badRequest.Value);
            Assert.Equal("Cannot delete a category that has products!", response.Message);
        }

        [Fact]
        public async Task Update_ReturnsSuccess_WhenCategoryIsValid()
        {
            var id = Guid.NewGuid();
            var dto = new CategoryUpdateDTO { Name = "Updated", Description = "Updated Desc" };
            var existing = new Category { CategoryId = id, Name = "Old", Description = "Old Desc" };

            _mockUnitOfWork.Setup(x => x.Categories.GetByIdAsync(id)).ReturnsAsync(existing);
            _mockUnitOfWork.Setup(x => x.Categories.categoryNameExist(dto.Name)).ReturnsAsync(false);
            _mockMapper.Setup(m => m.Map<CategoryDTO>(It.IsAny<Category>())).Returns(new CategoryDTO { Name = dto.Name });

            var result = await _controller.Update(id, dto);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsAssignableFrom<ApiResponse>(okResult.Value);
            Assert.True(response.Success);
            Assert.Equal("Category updated successfully", response.Message);
        }
    }
}
