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

    public async Task<IActionResult> ProductIndex()
    {
        var products = await _productService.ProductGetAll("");
        return View(products);
    }

    public IActionResult ProductCreate()
    {
        return View();
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> ProductCreate(ProductModel model)
    {
        if (ModelState.IsValid)
        {
            string token = await HttpContext.GetTokenAsync("access_token") ?? string.Empty;
            var response = await _productService.ProductCreate(model, token);
            if (response != null) return RedirectToAction(
                    nameof(ProductIndex));
        }
        return View(model);
    }

    public async Task<IActionResult> ProductUpdate(int id)
    {
        string token = await HttpContext.GetTokenAsync("access_token") ?? string.Empty;
        var model = await _productService.ProductGetById(id, token);
        if (model != null) return View(model);
        return NotFound();
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> ProductUpdate(ProductModel model)
    {
        if (ModelState.IsValid)
        {
            string token = await HttpContext.GetTokenAsync("access_token") ?? string.Empty;
            var response = await _productService.ProductUpdate(model, token);
            if (response != null) return RedirectToAction(
                 nameof(ProductIndex));
        }
        return View(model);
    }

    [Authorize]
    public async Task<IActionResult> ProductDelete(int id)
    {
        string token = await HttpContext.GetTokenAsync("access_token") ?? string.Empty;
        var model = await _productService.ProductGetById(id, token);
        if (model != null) return View(model);
        return NotFound();
    }

    [HttpPost]
    [Authorize(Roles = Role.Admin)]
    public async Task<IActionResult> ProductDelete(ProductModel model)
    {
        string token = await HttpContext.GetTokenAsync("access_token") ?? string.Empty;
        var response = await _productService.ProductDelete(model.Id, token);
        if (response) return RedirectToAction(
                nameof(ProductIndex));
        return View(model);
    }
}