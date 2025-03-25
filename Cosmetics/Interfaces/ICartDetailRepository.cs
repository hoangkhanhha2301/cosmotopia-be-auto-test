using Cosmetics.DTO.Cart;
using Cosmetics.Models;

namespace Cosmetics.Interfaces
{
    public interface ICartDetailRepository : IGenericRepository<CartDetail>
    {
        Task<CartDetailDTO> AddToCartAsync(CartDetailInputDTO cartDetailDto, int userId);
        Task<IEnumerable<CartDetailDTO>> GetCartAsync(int userId);
        Task<CartDetailDTO> GetCartItemAsync(Guid productId, int userId);
        Task<bool> UpdateCartItemAsync(CartDetailUpdateDTO cartDetailDto, int userId);
        Task<bool> RemoveFromCartAsync(Guid productId, int userId);
    }
}
