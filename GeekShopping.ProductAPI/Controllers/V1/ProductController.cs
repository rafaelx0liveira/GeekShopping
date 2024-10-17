using Asp.Versioning;
using GeekShopping.ProductAPI.Data.ValueObjects;
using GeekShopping.ProductAPI.Repository.Interfaces;
using GeekShopping.ProductAPI.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GeekShopping.ProductAPI.Controllers.V1
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    public class ProductController : ControllerBase
    {
        private IProductRepository _productRepository;

        public ProductController(IProductRepository productRepository)
        {
            _productRepository = productRepository ?? throw new System.ArgumentNullException(nameof(productRepository)); // Verify if the productRepository is null and throw an exception
        }

        [MapToApiVersion("1.0")]
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<ProductVO>> FindById(long id)
        {
            var product = await _productRepository.FindById(id);

            if (product == null) return NotFound();

            return Ok(product);
        }

        [MapToApiVersion("1.0")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductVO>>> FindAll()
        {
            var products = await _productRepository.FindAll();

            return Ok(products);
        }

        [MapToApiVersion("1.0")]
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<ProductVO>> Create([FromBody] ProductVO product)
        {
            if (product == null) return BadRequest();

            var createdProduct = await _productRepository.Create(product);

            if (createdProduct == null) return Conflict();

            return Ok(createdProduct);
        }

        [MapToApiVersion("1.0")]
        [HttpPut]
        [Authorize]
        public async Task<ActionResult<ProductVO>> Update([FromBody] ProductVO product)
        {
            if (product == null) return BadRequest();

            var updatedProduct = await _productRepository.Update(product);

            if (updatedProduct == null) return NotFound();

            return Ok(updatedProduct);
        }

        [MapToApiVersion("1.0")]
        [HttpDelete("{id}")]
        [Authorize(Roles = Role.AdminRole)]
        public async Task<ActionResult> Delete(long id)
        {
            var deleted = await _productRepository.Delete(id);

            if (!deleted) return NotFound();

            return Ok(deleted);
        }

    }
}
