using GeekShopping.Web.Models;
using GeekShopping.Web.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace GeekShopping.Web.Controllers;

public class HomeController(ILogger<HomeController> logger, IProductService productService, ICartService cartService) : Controller
{
    private readonly ILogger<HomeController> _logger = logger;
    private readonly IProductService _productService = productService;
    private readonly ICartService _cartService = cartService;

    public async Task<IActionResult> Index()
    {
        var products = await _productService.GetAll("");
        return View(products);
    }

    [Authorize]
    public async Task<IActionResult> Details(int id)
    {
        string token = await HttpContext.GetTokenAsync("access_token") ?? string.Empty;
        var model = await _productService.GetById(id, token);
        return View(model);
    }

    [HttpPost]
    [ActionName("Details")]
    [Authorize]
    public async Task<IActionResult> DetailsPost(ProductViewModel model)
    {
        string token = await HttpContext.GetTokenAsync("access_token") ?? string.Empty;

        CartViewModel cart = new()
        {
            CartHeader = new CartHeaderViewModel
            {
                UserId = User.Claims.Where(u => u.Type == "sub")?.FirstOrDefault()?.Value
            }
        };

        CartDetailViewModel cartDetail = new()
        {
            Count = model.Count,
            ProductId = model.Id,
            Product = await _productService.GetById(model.Id, token)
        };

        List<CartDetailViewModel> cartDetails = [];
        cartDetails.Add(cartDetail);
        cart.ListCartDetail = cartDetails;

        var response = await _cartService.AddItem(cart, token);
        if (response != null)
            return RedirectToAction(nameof(Index));
        return View(model);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    [Authorize]
    public async Task<IActionResult> Login()
    {
        var accessToken = await HttpContext.GetTokenAsync("access_token");
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Logout()
    {
        return SignOut("Cookies", "oidc");
    }
}