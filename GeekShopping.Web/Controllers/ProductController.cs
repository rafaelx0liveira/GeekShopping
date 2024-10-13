using GeekShopping.Web.Models;
using GeekShopping.Web.Services.Interfaces;
using GeekShopping.Web.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GeekShopping.Web.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));
        }

        [Authorize]
        public async Task<IActionResult> ProductIndex()
        {
            IEnumerable<ProductModel> products = await _productService.FindAllProducts();

            products ??= new List<ProductModel>();

            return View(products);
        }

        public async Task<IActionResult> ProductCreate()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ProductCreate(ProductModel product)
        {
            if (ModelState.IsValid)
            {
                if (product == null) return BadRequest();

                var response = await _productService.CreateProduct(product);

                if (response == null) return BadRequest();

                return RedirectToAction(nameof(ProductIndex));
            }

            return View(product);
        }

        public async Task<IActionResult> ProductUpdate(int id)
        {
            var product = await _productService.FindProductById(id);

            if (product == null) return NotFound();

            return View();
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ProductUpdate(ProductModel product)
        {
            if (ModelState.IsValid)
            {
                if (product == null) return BadRequest();

                var response = await _productService.UpdateProduct(product);

                if (response == null) return BadRequest();

                return RedirectToAction(nameof(ProductIndex));
            }

            return View(product);
        }

        [Authorize]
        public async Task<IActionResult> ProductDelete(int id)
        {
            var product = await _productService.FindProductById(id);

            if (product == null) return NotFound();

            return View(product);
        }

        [HttpPost]
        [Authorize(Roles = Role.AdminRole)]
        public async Task<IActionResult> ProductDelete(ProductModel product)
        {
            var response = await _productService.DeleteProductById(product.Id);

            if (!response) return View(product);

            return RedirectToAction(nameof(ProductIndex));
        }

    }
}
