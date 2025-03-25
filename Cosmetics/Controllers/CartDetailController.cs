using Cosmetics.DTO.Cart;
using Cosmetics.Repositories.UnitOfWork;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/cart")]
[Authorize]
public class CartDetailController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public CartDetailController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    [HttpPost("add")]
    public async Task<IActionResult> AddToCart([FromBody] CartDetailInputDTO cartDetailDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var result = await _unitOfWork.CartDetails.AddToCartAsync(cartDetailDto, 0); // UserId ignored, fetched internally
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message); // For invalid ProductId
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred: {ex.Message}");
        }
    }
    [HttpGet]
    public async Task<IActionResult> GetCart()
    {
        try
        {
            var cart = await _unitOfWork.CartDetails.GetCartAsync(0); // UserId ignored, fetched internally
            return Ok(cart);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred: {ex.Message}");
        }
    }

    [HttpGet("item/{productId}")]
    public async Task<IActionResult> GetCartItem(Guid productId)
    {
        try
        {
            var item = await _unitOfWork.CartDetails.GetCartItemAsync(productId, 0); // UserId ignored
            if (item == null)
                return NotFound($"No item found for ProductId: {productId}");
            return Ok(item);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred: {ex.Message}");
        }
    }

    [HttpPut("update")]
    public async Task<IActionResult> UpdateCartItem([FromBody] CartDetailUpdateDTO cartDetailDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var success = await _unitOfWork.CartDetails.UpdateCartItemAsync(cartDetailDto, 0); // UserId ignored
            if (!success)
                return NotFound($"No item found for ProductId: {cartDetailDto.ProductId}");
            return Ok("Cart item updated successfully");
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred: {ex.Message}");
        }
    }

    [HttpDelete("remove/{productId}")]
    public async Task<IActionResult> RemoveFromCart(Guid productId)
    {
        try
        {
            var success = await _unitOfWork.CartDetails.RemoveFromCartAsync(productId, 0); // UserId ignored
            if (!success)
                return NotFound($"No item found for ProductId: {productId}");
            return Ok("Item removed from cart successfully");
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred: {ex.Message}");
        }
    }
}
