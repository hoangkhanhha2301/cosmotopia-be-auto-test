using AutoMapper;
using Cosmetics.DTO.Brand;
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
    public class BrandController : ControllerBase
    {
        private readonly IBrand _brandRepo;
        private readonly IMapper _mapper;

        public BrandController(IBrand brandRepo, IMapper mapper)
        {
            _brandRepo = brandRepo;
            _mapper = mapper;
        }

        [HttpGet]
        [Route("GetAllBrand")]
        public async Task<IActionResult> GetAll()
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

            var brands = await _brandRepo.GetAllAsync();
            var brandDTO = _mapper.Map<List<BrandDTO>>(brands);
            return Ok(new ApiResponse
            {
                Success = true,
                Data = brandDTO
            });
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

            var brand = await _brandRepo.GetByIdAsync(id);
            if (brand == null)
            {
                return Ok(new ApiResponse
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
        public async Task<IActionResult> Create([FromBody] CreateBrandDTO brandDTO)
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

            await _brandRepo.CreateAsync(brandModel);
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

            var brand = await _brandRepo.DeleteByIdAsync(id);

            if (brand == null)
            {
                return Ok(new ApiResponse
                {
                    Success = false,
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "Brand does not exist!",
                });
            }

            return NoContent();
        }

        [HttpPut]
        [Route("UpdateBrandBy/{id:guid}")]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateBrandDTO brandDTO)
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

            var update = await _brandRepo.UpdateAsync(id, brandDTO);
            if (update == null)
            {
                return Ok(new ApiResponse
                {
                    Success = false,
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "Brand not found!"
                });
            }

            return Ok(new ApiResponse
            {
                Success = true,
                Data = _mapper.Map<BrandDTO>(update)
            });
        }
    }
}
