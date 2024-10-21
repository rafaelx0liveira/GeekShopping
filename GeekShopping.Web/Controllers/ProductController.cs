using GeekShopping.Web.Models;
using GeekShopping.Web.Services.Interfaces;
using GeekShopping.Web.Utils;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GeekShopping.Web.Controllers;

public class ProductController : Controller
{
    private readonly IProductService _productService;

    public ProductController(IProductService productService)
    {
        _productService = productService ?? throw new ArgumentNullException(nameof(productService));
    }

    public async Task<IActionResult> Index()
    {
        var products = await _productService.GetAll("");
        return View(products);
    }

    public IActionResult Create()
    {
        return View();
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create(ProductViewModel model)
    {
        if (ModelState.IsValid)
        {
            string token = await HttpContext.GetTokenAsync("access_token") ?? string.Empty;
            var response = await _productService.Create(model, token);
            if (response != null) return RedirectToAction(
                 nameof(Index));
        }
        return View(model);
    }

    public async Task<IActionResult> Update(int id)
    {
        string token = await HttpContext.GetTokenAsync("access_token") ?? string.Empty;
        var model = await _productService.GetById(id, token);
        if (model != null) return View(model);
        return NotFound();
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Update(ProductViewModel model)
    {
        if (ModelState.IsValid)
        {
            string token = await HttpContext.GetTokenAsync("access_token") ?? string.Empty;
            var response = await _productService.Update(model, token);
            if (response != null) return RedirectToAction(
                 nameof(Index));
        }
        return View(model);
    }

    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        string token = await HttpContext.GetTokenAsync("access_token") ?? string.Empty;
        var model = await _productService.GetById(id, token);
        if (model != null) return View(model);
        return NotFound();
    }

    [HttpPost]
    [Authorize(Roles = Role.Admin)]
    public async Task<IActionResult> Delete(ProductViewModel model)
    {
        string token = await HttpContext.GetTokenAsync("access_token") ?? string.Empty;
        var response = await _productService.Delete(model.Id, token);
        if (response) return RedirectToAction(
                nameof(Index));
        return View(model);
    }
}