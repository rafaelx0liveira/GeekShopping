using GeekShopping.Web.Models;
using GeekShopping.Web.Services.Interfaces;
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
    }
}
