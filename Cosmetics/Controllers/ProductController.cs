using AutoMapper;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
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
        private readonly Cloudinary _cloudinary;

        public ProductController(IProduct productRepo, IMapper mapper, Cloudinary cloudinary)
        {
            _productRepo = productRepo;
            _mapper = mapper;
            _cloudinary = cloudinary;
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
        public async Task<IActionResult> Create([FromForm] CreateProductDTO productDTO, List<IFormFile> imageFiles)
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

            var imageUrls = new List<string>();
            if(imageFiles != null && imageFiles.Count > 0)
            {
                foreach(var imageFile in imageFiles) 
                { 
                    var uploadParams = new ImageUploadParams()
                    {
                        File = new FileDescription(imageFile.FileName, imageFile.OpenReadStream())
                    };
                    var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                    imageUrls.Add(uploadResult.SecureUrl.ToString());
                }
            }

            var productModel = new Product
            {
                ProductId = Guid.NewGuid(),
                Name = productDTO.Name,
                Description = productDTO.Description,
                Price = productDTO.Price,
                StockQuantity = productDTO.StockQuantity,
                ImageUrls = string.Join(", ", imageUrls),
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
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromForm] UpdateProductDTO productDTO, List<IFormFile> imageFiles)
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

            var existingProduct = await _productRepo.GetByIdAsync(id);
            if(existingProduct == null)
            {
                return NotFound(new ApiResponse
                {
                    Success = false,
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "Product not found!",
                });
            }

            //Delete old image on Cloudinary
            if(!string.IsNullOrEmpty(existingProduct.ImageUrls))
            {
                var oldImageUrls = existingProduct.ImageUrls.Split(",");
                foreach(var imageUrl in oldImageUrls)
                {
                    var publicId = imageUrl.Split("/").Last().Split(".").First();
                    var deletionParams = new DeletionParams(publicId);
                    await _cloudinary.DestroyAsync(deletionParams);
                }
            }

            //Upload image on Cloudinary
            var imageUrls = new List<string>();
            if(imageFiles != null && imageFiles.Count > 0)
            {
                foreach(var imageFile in imageFiles)
                {
                    var uploadParams = new ImageUploadParams()
                    {
                        File = new FileDescription(imageFile.FileName, imageFile.OpenReadStream())
                    };

                    var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                    imageUrls.Add(uploadResult.SecureUrl.ToString());
                }
            }

            productDTO.ImageUrls = string.Join(", ", imageUrls);

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
                StatusCode = StatusCodes.Status200OK,
                Message = "Updated Product Successfully.",
                Data = _mapper.Map<ProductDTO>(update)
            });
        }
    }
}
