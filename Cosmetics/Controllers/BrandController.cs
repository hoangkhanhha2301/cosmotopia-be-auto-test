using AutoMapper;
using Cosmetics.DTO.Brand;
using Cosmetics.DTO.User;
using Cosmetics.Models;
using Cosmetics.Repositories.UnitOfWork;
using Microsoft.AspNetCore.Mvc;

namespace Cosmetics.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BrandController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public BrandController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpGet]
        [Route("GetAllBrand")]
        public async Task<IActionResult> GetBrands([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
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

           if(page < 1 || pageSize < 1)
            {
                return BadRequest("Page and pageSize must be greater than 0.");
            }

            var brands = await _unitOfWork.Brands.GetAllAsync();
            var totalCount = brands.Count();
            var paginationBrands = brands.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            var brandDTO = _mapper.Map<List<BrandDTO>>(paginationBrands);

            var response = new
            {
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                CurrentPages = page,
                PageSize = pageSize,
                Brands = brandDTO
            };

            return Ok(response);
        }

        [HttpGet]
        [Route("GetBrandBy/{id:guid}")]
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

            var brand = await _unitOfWork.Brands.GetByIdAsync(id);
            if (brand == null)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "Brand not found!"
                });
            }

            return Ok(new ApiResponse
            {
                Success = true,
                StatusCode = StatusCodes.Status200OK,
                Data = _mapper.Map<BrandDTO>(brand)
            });
        }

        [HttpPost]
        [Route("CreateBrand")]
        public async Task<IActionResult> Create([FromBody] BrandCreateDTO brandDTO)
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

            var brandModel = new Brand
            {
                BrandId = Guid.NewGuid(),
                Name = brandDTO.Name,
                IsPremium = brandDTO.IsPremium,
                CreatedAt = DateTime.Now,
            };

            if(await _unitOfWork.Brands.brandNameExist(brandModel.Name))
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = "BrandName already exist!",
                });
            }

            await _unitOfWork.Brands.AddAsync(brandModel);
            await _unitOfWork.CompleteAsync();

            return Ok(new ApiResponse
            {
                Success = true,
                StatusCode = StatusCodes.Status201Created,
                Message = "Brand Category Successfully.",
                Data = _mapper.Map<BrandDTO>(brandModel)
            });
        }

        [HttpDelete]
        [Route("DeleteBrandBy/{id:guid}")]
        public async Task<IActionResult> Delete([FromRoute] Guid id)
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

            var hasProducts = await _unitOfWork.Brands.brandHasProducts(id);

            if (hasProducts)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = "Cannot delete a brand that has products!",
                    StatusCode = StatusCodes.Status403Forbidden,
                }); 
            }

            var brand = await _unitOfWork.Brands.GetByIdAsync(id);

            if (brand == null)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "Brand does not exist!",
                });
            }

             _unitOfWork.Brands.Delete(brand);
            await _unitOfWork.CompleteAsync();

            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Brand deleted successfully"
            });
        }

        [HttpPut]
        [Route("UpdateBrandBy/{id:guid}")]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] BrandUpdateDTO brandDTO)
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

            var update = await _unitOfWork.Brands.GetByIdAsync(id);

            if (update == null)
            {
                return Ok(new ApiResponse
                {
                    Success = false,
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "Brand not found!"
                });
            }

            update.Name = brandDTO.Name;
            update.IsPremium = brandDTO.IsPremium;

            //if(await _unitOfWork.Brands.brandNameExist(update.Name))
            //{
            //    return BadRequest(new ApiResponse
            //    {
            //        Success = false,
            //        Message = "BrandName already exist!"
            //    });
            //}

            _unitOfWork.Brands.UpdateAsync(update);
            await _unitOfWork.CompleteAsync();

            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Brand updated successfully",
                Data = _mapper.Map<BrandDTO>(update)
            });
        }
    }
}
