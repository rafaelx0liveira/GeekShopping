using GeekShopping.Web.Models;
using GeekShopping.Web.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GeekShopping.Web.Controllers;

public class CartController(IProductService productService, ICartService cartService, ICouponService couponService) : Controller
{
    private readonly IProductService _productService = productService;
    private readonly ICartService _cartService = cartService;
    private readonly ICouponService _couponService = couponService;

    [Authorize]
    public async Task<IActionResult> Index()
    {
        return View(await GetByUserId());
    }

    public async Task<IActionResult> Remove(int id)
    {
        string token = await HttpContext.GetTokenAsync("access_token") ?? string.Empty;
        string userId = User.Claims.Where(u => u.Type == "sub")?.FirstOrDefault()?.Value!;

        var response = await _cartService.RemoveItem(id, token);

        if (response) return RedirectToAction(nameof(Index));
        return View();
    }

    private async Task<CartViewModel?> GetByUserId()
    {
        string token = await HttpContext.GetTokenAsync("access_token") ?? string.Empty;
        string userId = User.Claims.Where(u => u.Type == "sub")?.FirstOrDefault()?.Value!;

        var response = await _cartService.GetByUserId(userId, token);

        if (response?.CartHeader != null)
        {
            if (!string.IsNullOrEmpty(response.CartHeader.CouponCode))
            {
                var coupon = await _couponService.GetCoupon(response.CartHeader.CouponCode, token);
                if (coupon?.Code != null)
                    response.CartHeader.DiscountAmount = coupon.DiscountAmount;
            }
            foreach (var detail in response.ListCartDetail!)
            {
                response.CartHeader.PurchaseAmount += (detail.Product!.Price * detail.Count);
            }
            response.CartHeader.PurchaseAmount -= response.CartHeader.DiscountAmount;
        }

        return response;
    }

    [HttpPost]
    [ActionName("ApplyCoupon")]
    public async Task<IActionResult> ApplyCoupon(CartViewModel model)
    {
        string token = await HttpContext.GetTokenAsync("access_token") ?? string.Empty;
        string userId = User.Claims.Where(u => u.Type == "sub")?.FirstOrDefault()?.Value!;

        var response = await _cartService.ApplyCoupon(model, token);

        if (response) return RedirectToAction(nameof(Index));
        return View();
    }

    [HttpPost]
    [ActionName("RemoveCoupon")]
    public async Task<IActionResult> RemoveCoupon()
    {
        string token = await HttpContext.GetTokenAsync("access_token") ?? string.Empty;
        string userId = User.Claims.Where(u => u.Type == "sub")?.FirstOrDefault()?.Value!;

        var response = await _cartService.RemoveCoupon(userId, token);

        if (response) return RedirectToAction(nameof(Index));
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> Checkout()
    {
        return View(await GetByUserId());
    }

    [HttpPost]
    public async Task<IActionResult> Checkout(CartViewModel model)
    {
        string token = await HttpContext.GetTokenAsync("access_token") ?? string.Empty;

        var response = await _cartService.Checkout(model.CartHeader!, token);

        if (response != null) return RedirectToAction(nameof(Confirmation));
        return View(model);
    }

    [HttpGet]
    public IActionResult Confirmation()
    {
        return View();
    }
}