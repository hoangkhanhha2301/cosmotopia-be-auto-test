using AutoMapper;
using Cosmetics.DTO.Category;
using Cosmetics.DTO.User;
using Cosmetics.Interfaces;
using Cosmetics.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Cosmetics.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ICategory _categoryRepo;
        private readonly IMapper _mapper;

        public CategoryController(ICategory categoryRepo, IMapper mapper) 
        {
            _categoryRepo = categoryRepo;
            _mapper = mapper;
        }

        [HttpGet]
        [Route("GetAllCategory")]
        public async Task<IActionResult> GetAll()
        {
            if(!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse 
                {
                    Success = false,
                    Message = "Invalid data",
                    Data = ModelState
                });
            }

            var categories = await _categoryRepo.GetAllAsync();
            var categoryDTO = _mapper.Map<List<CategoryDTO>>(categories);

            return Ok(new ApiResponse 
            {
                Success = true,
                StatusCode = StatusCodes.Status200OK,
                Data = categoryDTO
            });
        }

        [HttpGet]
        [Route("GetCategoryBy/{id:guid}")]
        public async Task<IActionResult> GetById([FromRoute] Guid id)
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

            var category = await _categoryRepo.GetByIdAsync(id);
            if (category == null)
            {
                return Ok(new ApiResponse
                {
                    Success = false,
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "Category not found!"
                });
            }

            return Ok(new ApiResponse
            { 
                Success = true,
                StatusCode = StatusCodes.Status200OK,
                Data = _mapper.Map<CategoryDTO>(category)
            });
        }

        [HttpPost]
        [Route("CreateCategory")]
        public async Task<IActionResult> Create([FromBody] CreateCategoryDTO categoryDTO)
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

            var categoryModel = new Category
            {
                CategoryId = Guid.NewGuid(),
                Name = categoryDTO.Name,
                Description = categoryDTO.Description,
                CreatedAt = DateTime.Now,
            };

            await _categoryRepo.CreateAsync(categoryModel);
            return Ok(new ApiResponse
            {
                Success = true,
                StatusCode = StatusCodes.Status201Created,
                Message = "Created Category Successfully.",
                Data = _mapper.Map<CategoryDTO>(categoryModel)
            });
        }

        [HttpDelete]
        [Route("DeleteCategoryBy/{id:guid}")]
        public async Task<IActionResult> DeleteById([FromRoute] Guid id)
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

            var hasProducts = await _categoryRepo.CategoryHasProducts(id);

            if(hasProducts)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = "Cannot delete a category that has products!",
                    StatusCode = StatusCodes.Status403Forbidden,
                });
            }

            var category = await _categoryRepo.DeleteAsync(id);

            if(category == null)
            {
                return Ok(new ApiResponse
                {
                    Success = false,
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "Category does not exist!",
                });
            }

            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Category deleted successfully."
            });
        }

        [HttpPut]
        [Route("UpdateCategoryBy/{id:guid}")]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateCategoryDTO categoryDTO )
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

            var update = await _categoryRepo.UpdateAsync(id, categoryDTO);
            if (update == null)
            {
                return Ok(new ApiResponse
                {
                    Success = false,
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "Category not found!"
                });
            }

            return Ok(new ApiResponse
            {
                Success = true,
                Data = _mapper.Map<CategoryDTO>(update)
            });
        }
    }
}
