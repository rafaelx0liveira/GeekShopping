using GeekShopping.ProductAPI.Data.ValueObjects;
using GeekShopping.ProductAPI.Repository.Interfaces;
using GeekShopping.ProductAPI.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GeekShopping.ProductAPI.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
public class ProductController : ControllerBase
{
    private IProductRepository _repository;

    public ProductController(IProductRepository repository)
    {
        _repository = repository ?? throw new
            ArgumentNullException(nameof(repository));
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductVO>>> GetAll()
    {
        var products = await _repository.GetAll();
        return Ok(products);
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<ActionResult<ProductVO>> GetById(long id)
    {
        var product = await _repository.GetById(id);
        if (product == null) return NotFound();
        return Ok(product);
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<ProductVO>> Create([FromBody] ProductVO vo)
    {
        if (vo == null) return BadRequest();
        var product = await _repository.Create(vo);
        return Ok(product);
    }

    [HttpPut]
    [Authorize]
    public async Task<ActionResult<ProductVO>> Update([FromBody] ProductVO vo)
    {
        if (vo == null) return BadRequest();
        var product = await _repository.Update(vo);
        return Ok(product);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = Role.Admin)]
    public async Task<ActionResult> Delete(long id)
    {
        var status = await _repository.Delete(id);
        if (!status) return BadRequest();
        return Ok(status);
    }
}