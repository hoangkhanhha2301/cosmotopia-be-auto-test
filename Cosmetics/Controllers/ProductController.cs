using AutoMapper;
using Cosmetics.DTO.Product;
using Cosmetics.DTO.User;
using Cosmetics.Helpers;
using Cosmetics.Interfaces;
using Cosmetics.Models;
using Microsoft.AspNetCore.Mvc;

namespace Cosmetics.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProduct _productRepo;
        private readonly IMapper _mapper;

        public ProductController(IProduct productRepo, IMapper mapper)
        {
            _productRepo = productRepo;
            _mapper = mapper;
        }

        [HttpGet]
        [Route("GetAllProduct")]
        public async Task<IActionResult> GetAll([FromQuery] QueryObject query)
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

            var products = await _productRepo.GetAllAsync(query);
            var productDTO = _mapper.Map<List<ProductDTO>>(products);
            return Ok(new ApiResponse
            {
                Success = true,
                StatusCode = StatusCodes.Status200OK,
                Data = productDTO
            });
        }

        [HttpGet]
        [Route("GetProductBy/{id:guid}")]
        public async Task<IActionResult> GetById([FromRoute] Guid id)
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

            var product = await _productRepo.GetByIdAsync(id);

            if(product == null)
            {
                return Ok(new ApiResponse
                {
                    Success = false,
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "Product not found!"
                });
            }

            return Ok(new ApiResponse 
            { 
                Success = true,
                StatusCode = StatusCodes.Status200OK,
                Data = _mapper.Map<ProductDTO>(product)
            });
        }

        [HttpDelete]
        [Route("DeleteProduct/{id:guid}")]
        public async Task<IActionResult> DeleteById([FromRoute] Guid id)
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

            var product = await _productRepo.DeleteAsync(id);
            if(product == null)
            {
                return Ok(new ApiResponse
                {
                    Success = false,
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "Product does not exist!",
                }); 
            }

            return NoContent();
        }

        [HttpPost]
        [Route("CreateProduct")]
        public async Task<IActionResult> Create([FromBody] CreateProductDTO productDTO)
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

            if(!await _productRepo.CategoryExist(productDTO.CategoryId.Value)) 
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    StatusCode= StatusCodes.Status404NotFound,
                    Message = "Category does not exist!",
                });
            }

            if(!await _productRepo.BranchExist(productDTO.BrandId.Value))
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "Branch does not exist!",
                });
            }

            var productModel = new Product
            {
                ProductId = Guid.NewGuid(),
                Name = productDTO.Name,
                Description = productDTO.Description,
                Price = productDTO.Price,
                StockQuantity = productDTO.StockQuantity,
                ImageUrls = productDTO.ImageUrls,
                CommissionRate = productDTO.CommissionRate,
                CategoryId = productDTO.CategoryId,
                BrandId = productDTO.BrandId,
                CreateAt = DateTime.UtcNow,
                IsActive = true,
            };

            await _productRepo.CreateAsync(productModel);
            return Ok(new ApiResponse
            {
                Success = true,
                StatusCode = StatusCodes.Status200OK,
                Message = "Created Product Successfully.",
                Data = _mapper.Map<ProductDTO>(productModel)
            });
        }

        [HttpPut]
        [Route("UpdateProduct/{id:guid}")]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateProductDTO productDTO)
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

            var update = await _productRepo.UpdateAsync(id, productDTO);

            if(update == null)
            {
                return Ok(new ApiResponse
                {
                    Success = false,
                    Message = "Product not found!"
                });
            }

            return Ok(new ApiResponse
            {
                Success = true,
                Data = _mapper.Map<ProductDTO>(update)
            });
        }
    }
}
