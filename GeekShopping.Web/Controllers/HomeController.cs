using GeekShopping.Web.Models;
using GeekShopping.Web.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace GeekShopping.Web.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IProductService _productService;
    private readonly ICartService _cartService;

    public HomeController(ILogger<HomeController> logger, 
        IProductService productService,
        ICartService cartService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _productService = productService ?? throw new ArgumentNullException(nameof(productService));
        _cartService = cartService ?? throw new ArgumentNullException(nameof(cartService));
    }

    public async Task<IActionResult> Index()
    {
        var products = await _productService.ProductGetAll("");
        return View(products);
    }

    [Authorize]
    public async Task<IActionResult> Details(int id)
    {
        var accessToken = await HttpContext.GetTokenAsync("access_token") ?? string.Empty;
        var model = await _productService.ProductGetById(id, accessToken);
        return View(model);
    }

    [HttpPost]
    [ActionName("Details")]
    [Authorize]
    public async Task<IActionResult> DetailsPost(ProductViewModel model)
    {
        var accessToken = await HttpContext.GetTokenAsync("access_token") ?? string.Empty;

        CartViewModel cart = new()
        {
            CartHeader = new()
            {
                UserId = User.Claims.Where(u => u.Type == "sub").FirstOrDefault()?.Value
            },
        };

        CartDetailViewModel cartDetail = new()
        {
            Count = model.Count,
            ProductId = model.Id,
            Product = await _productService.ProductGetById(model.Id, accessToken),
            CartHeader = cart.CartHeader
        };

        List<CartDetailViewModel> cartDetailsList = new List<CartDetailViewModel>();
        
        cartDetailsList.Add(cartDetail);

        cart.CartDetails = cartDetailsList;

        var response = await _cartService.AddItemToCart(cart, accessToken);

        if (response != null)
            return RedirectToAction(nameof(Index));

        return View(model);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    public IActionResult Logout()
    {
        return SignOut("Cookies", "oidc");
    }

    [Authorize]
    public async Task<IActionResult> Login()
    {
        var accessToken = await HttpContext.GetTokenAsync("access_token");

        return RedirectToAction(nameof(Index));
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}