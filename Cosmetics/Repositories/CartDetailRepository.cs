using Cosmetics.DTO.Brand;
using Cosmetics.DTO.Cart;
using Cosmetics.DTO.Category;
using Cosmetics.DTO.Product;
using Cosmetics.Interfaces;
using Cosmetics.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Cosmetics.Repositories
{
    public class CartDetailRepository : GenericRepository<CartDetail>, ICartDetailRepository
    {
        private readonly ComedicShopDBContext _context; // Your DbContext
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CartDetailRepository(ComedicShopDBContext context, IHttpContextAccessor httpContextAccessor)
            : base(context)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                throw new UnauthorizedAccessException("User is not logged in or UserId is invalid.");
            return userId;
        }

        public async Task<CartDetailDTO> AddToCartAsync(CartDetailInputDTO cartDetailDto, int userId)
        {
            userId = GetCurrentUserId(); // Override with logged-in user

            // Check if the product exists
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .FirstOrDefaultAsync(p => p.ProductId == cartDetailDto.ProductId);
            if (product == null)
                throw new ArgumentException($"Product with ID {cartDetailDto.ProductId} not found.");

            // Check if item already exists in cart
            var existingItem = await _context.CartDetails
                .Include(cd => cd.Product)
                .ThenInclude(p => p.Category)
                .Include(cd => cd.Product)
                .ThenInclude(p => p.Brand)
                .FirstOrDefaultAsync(cd => cd.UserId == userId && cd.ProductId == cartDetailDto.ProductId);

            if (existingItem != null)
            {
                var newQuantity = existingItem.Quantity + cartDetailDto.Quantity;
                if (newQuantity > existingItem.Product.StockQuantity)
                {
                    existingItem.Quantity = existingItem.Product.StockQuantity ?? 0;
                }
                else
                {
                    existingItem.Quantity = newQuantity;
                }

                    _context.CartDetails.Update(existingItem);
            }
            else
            {
                var cartDetail = new CartDetail
                {
                    UserId = userId,
                    ProductId = cartDetailDto.ProductId,
                    Quantity = cartDetailDto.Quantity,
                    CreatedAt = DateTime.UtcNow // Fetched from system
                };
                await _context.CartDetails.AddAsync(cartDetail);
            }

            await _context.SaveChangesAsync();

            // Fetch the updated item to return with full details
            var updatedItem = await _context.CartDetails
                .Include(cd => cd.Product)
                .ThenInclude(p => p.Category)
                .Include(cd => cd.Product)
                .ThenInclude(p => p.Brand)
                .FirstOrDefaultAsync(cd => cd.UserId == userId && cd.ProductId == cartDetailDto.ProductId);

            return MapToCartDetailDTO(updatedItem);
        }

        public async Task<IEnumerable<CartDetailDTO>> GetCartAsync(int userId)
        {
            userId = GetCurrentUserId(); // Override with logged-in user
            var cartItems = await _context.CartDetails
                .Include(cd => cd.Product)
                .ThenInclude(p => p.Category)
                .Include(cd => cd.Product)
                .ThenInclude(p => p.Brand)
                .Where(cd => cd.UserId == userId)
                .ToListAsync();

            return cartItems.Select(cd => MapToCartDetailDTO(cd));
        }

        public async Task<CartDetailDTO> GetCartItemAsync(Guid productId, int userId)
        {
            userId = GetCurrentUserId(); // Override with logged-in user
            var cartItem = await _context.CartDetails
                .Include(cd => cd.Product)
                .ThenInclude(p => p.Category)
                .Include(cd => cd.Product)
                .ThenInclude(p => p.Brand)
                .FirstOrDefaultAsync(cd => cd.UserId == userId && cd.ProductId == productId);

            if (cartItem == null)
                return null;

            return MapToCartDetailDTO(cartItem);
        }

        public async Task<bool> UpdateCartItemAsync(CartDetailUpdateDTO cartDetailUpdateDto, int userId)
        {
            userId = GetCurrentUserId(); // Override with logged-in user
            var cartItem = await _context.CartDetails.Include(cd => cd.Product)
                .FirstOrDefaultAsync(cd => cd.UserId == userId && cd.ProductId == cartDetailUpdateDto.ProductId);

            if (cartItem == null)
                return false;
            if(cartItem.Product.StockQuantity < cartDetailUpdateDto.Quantity)
            {
                return false;
            }
            cartItem.Quantity = cartDetailUpdateDto.Quantity;
            _context.CartDetails.Update(cartItem);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveFromCartAsync(Guid productId, int userId)
        {
            userId = GetCurrentUserId(); // Override with logged-in user
            var cartItem = await _context.CartDetails
                .FirstOrDefaultAsync(cd => cd.UserId == userId && cd.ProductId == productId);

            if (cartItem == null)
                return false;

            _context.CartDetails.Remove(cartItem);
            await _context.SaveChangesAsync();
            return true;
        }

        private CartDetailDTO MapToCartDetailDTO(CartDetail cartDetail)
        {
            return new CartDetailDTO
            {
                ProductId = cartDetail.ProductId,
                Quantity = cartDetail.Quantity,
                CreatedAt = cartDetail.CreatedAt,
                Product = new ProductDTO
                {
                    ProductId = cartDetail.Product.ProductId,
                    Name = cartDetail.Product.Name,
                    Description = cartDetail.Product.Description,
                    Price = cartDetail.Product.Price,
                    StockQuantity = cartDetail.Product.StockQuantity,
                    ImageUrls = cartDetail.Product.ImageUrls,
                    CommissionRate = cartDetail.Product.CommissionRate,
                    CategoryId = cartDetail.Product.CategoryId,
                    BrandId = cartDetail.Product.BrandId,
                    CreateAt = cartDetail.Product.CreateAt,
                    UpdatedAt = cartDetail.Product.UpdatedAt,
                    IsActive = cartDetail.Product.IsActive,
                    Category = cartDetail.Product.Category != null ? new CategoryDTO
                    {
                        CategoryId = cartDetail.Product.Category.CategoryId,
                        Name = cartDetail.Product.Category.Name
                        // Add other CategoryDTO properties
                    } : null,
                    Brand = cartDetail.Product.Brand != null ? new BrandDTO
                    {
                        BrandId = cartDetail.Product.Brand.BrandId,
                        Name = cartDetail.Product.Brand.Name
                        // Add other BrandDTO properties
                    } : null
                }
            };
        }
    }
}
