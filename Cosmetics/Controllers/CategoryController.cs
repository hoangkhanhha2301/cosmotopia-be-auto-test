using AutoMapper;
using Cosmetics.DTO.Category;
using Cosmetics.DTO.User;
using Cosmetics.Models;
using Cosmetics.Repositories.UnitOfWork;
using Microsoft.AspNetCore.Mvc;

namespace Cosmetics.Controllers
{
	[Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CategoryController(IUnitOfWork unitOfWork, IMapper mapper) 
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpGet]
        [Route("GetAllCategory")]
        public async Task<IActionResult> GetCategories([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
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

            if(page < 1 || pageSize < 1)
            {
                return BadRequest("Page and pageSize must be greater than 0.");
            }

            var categories = await _unitOfWork.Categories.GetAllAsync();
            var totalCount = categories.Count();
            var paginationCategories = categories.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            var categoryDTO = _mapper.Map<List<CategoryDTO>>(paginationCategories);

            var response = new
            {
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                CurrentPage = page,
                PageSize = pageSize,
                Categories = categoryDTO
            };

            return Ok(response);
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

            var category = await _unitOfWork.Categories.GetByIdAsync(id);
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
        public async Task<IActionResult> Create([FromBody] CategoryCreateDTO categoryDTO)
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

            if(await _unitOfWork.Categories.categoryNameExist(categoryModel.Name))
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = "CategoryName already exist!"
                });
            }

            await _unitOfWork.Categories.AddAsync(categoryModel);
            await _unitOfWork.CompleteAsync();

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

            var hasProducts = await _unitOfWork.Categories.categoryHasProducts(id);

            if(hasProducts)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = "Cannot delete a category that has products!",
                    StatusCode = StatusCodes.Status403Forbidden,
                });
            }

            var category = await _unitOfWork.Categories.GetByIdAsync(id);

            if(category == null)
            {
                return Ok(new ApiResponse
                {
                    Success = false,
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "Category does not exist!",
                });
            }

            _unitOfWork.Categories.Delete(category);
            await _unitOfWork.CompleteAsync();

            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Category deleted successfully."
            });
        }

        [HttpPut]
        [Route("UpdateCategoryBy/{id:guid}")]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] CategoryUpdateDTO categoryDTO )
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

            var update = await _unitOfWork.Categories.GetByIdAsync(id);

            if (update == null)
            {
                return Ok(new ApiResponse
                {
                    Success = false,
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "Category not found!"
                });
            }

            update.Name = categoryDTO.Name;
            update.Description = categoryDTO.Description;

            if(await _unitOfWork.Categories.categoryNameExist(update.Name))
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = "CategoryName already exist!"
                });
            }

            _unitOfWork.Categories.UpdateAsync(update);
            await _unitOfWork.CompleteAsync();

            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Category updated successfully",
                Data = _mapper.Map<CategoryDTO>(update)
            });
        }
    }
}
