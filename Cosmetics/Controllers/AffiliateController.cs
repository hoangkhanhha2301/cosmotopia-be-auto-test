using Cosmetics.DTO.Affiliate;
using Cosmetics.Models;
using Cosmetics.Service.Affiliate;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class AffiliateController : ControllerBase
{
    private readonly IAffiliateService _affiliateService;
    private readonly IProductService _productService;

    public AffiliateController(IAffiliateService affiliateService, IProductService productService)
    {
        _affiliateService = affiliateService;
        _productService = productService;
    }

    // ✅ Hàm lấy UserId và Role từ Token
    private (int UserId, string Role) GetUserInfo()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || string.IsNullOrEmpty(roleClaim))
        {
            throw new UnauthorizedAccessException("Không tìm thấy UserId hoặc Role trong token.");
        }

        return (int.Parse(userIdClaim), roleClaim);
    }

    // ✅ Xem thu nhập và thống kê của Affiliate trong 1 tuần
    [HttpGet("income")]
    [Authorize(Roles = "Affiliates")]
    public async Task<IActionResult> GetAffiliateIncome()
    {
        try
        {
            var (userId, role) = GetUserInfo();
            var affiliateProfile = await _affiliateService.GetAffiliateProfile(userId);
            var income = await _affiliateService.GetWeeklyIncome(affiliateProfile.AffiliateProfileId);

            return Ok(income);
        }
        catch (Exception ex)
        {
            return Unauthorized(ex.Message);
        }
    }

    // ✅ Xem top 5 sản phẩm mang lại doanh thu cao nhất
    [HttpGet("top-products")]
    [Authorize(Roles = "Affiliates")]
    public async Task<IActionResult> GetTopProducts()
    {
        try
        {
            var (userId, role) = GetUserInfo();
            var affiliateProfile = await _affiliateService.GetAffiliateProfile(userId);
            var topProducts = await _affiliateService.GetTopProducts(affiliateProfile.AffiliateProfileId, 5);

            return Ok(topProducts);
        }
        catch (Exception ex)
        {
            return Unauthorized(ex.Message);
        }
    }

    // ✅ Tạo link Affiliate từ Product
    [HttpPost("generate-link")]
    [Authorize(Roles = "Affiliates")]
    public async Task<IActionResult> GenerateAffiliateLink([FromBody] GenerateAffiliateLinkDto dto)
    {
        try
        {
            var (userId, role) = GetUserInfo();
            var affiliateProfile = await _affiliateService.GetAffiliateProfile(userId);
            var link = await _affiliateService.GenerateAffiliateLink(affiliateProfile.AffiliateProfileId, dto.ProductId);

            return Ok(new { AffiliateLink = link });
        }
        catch (Exception ex)
        {
            return Unauthorized(ex.Message);
        }
    }

    // ✅ Theo dõi click vào link Affiliate
    [HttpGet("track-click")]
    public async Task<IActionResult> TrackClick([FromQuery] string referralCode)
    {
        var clickTracked = await _affiliateService.TrackClick(referralCode, DateTime.Now);
        if (!clickTracked)
        {
            return BadRequest("Invalid referral code or product.");
        }

        return Ok(new { Message = "Click tracked successfully!" });
    }
    [HttpGet("debug-token")]
    public IActionResult DebugToken()
    {
        var token = Request.Headers["Authorization"].ToString();
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var role = User.FindFirst(ClaimTypes.Role)?.Value;

        return Ok(new { Token = token, UserId = userId, Role = role });
    }

}
