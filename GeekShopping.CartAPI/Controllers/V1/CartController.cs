using Asp.Versioning;
using GeekShopping.CartAPI.Data.ValueObjects;
using GeekShopping.CartAPI.Repository.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GeekShopping.CartAPI.Controllers.V1
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    public class CartController : ControllerBase
    {
        private ICartRepository _cartRepository;

        public CartController(ICartRepository cartRepository)
        {
            _cartRepository = cartRepository 
                ?? 
                throw new System.ArgumentNullException(nameof(cartRepository)); // Verify if the productRepository is null and throw an exception
        }

        [MapToApiVersion("1.0")]
        [HttpGet("find-cart/{userId}")]
        public async Task<ActionResult<CartVO>> FindById(string userId)
        {
            var cart = await _cartRepository.FindCartByUserId(userId);

            if (cart == null) return NotFound();

            return Ok(cart);
        }

        [MapToApiVersion("1.0")]
        [HttpPost("add-cart")]
        public async Task<ActionResult<CartVO>> AddCart([FromBody] CartVO cartVO)
        {
            var cart = await _cartRepository.SaveOrUpdateCart(cartVO);

            if (cart == null) return NotFound();

            return Ok(cart);
        }

        [MapToApiVersion("1.0")]
        [HttpPut("update-cart")]
        public async Task<ActionResult<CartVO>> UpdateCart([FromBody] CartVO cartVO)
        {
            var cart = await _cartRepository.SaveOrUpdateCart(cartVO);

            if (cart == null) return NotFound();

            return Ok(cart);
        }

        [MapToApiVersion("1.0")]
        [HttpDelete("remove-cart/{cartDetailId}")]
        public async Task<ActionResult<bool>> RemoveCart(long cartDetailId)
        {
            var result = await _cartRepository.RemoveFromCart(cartDetailId);

            if (!result) return BadRequest();

            return Ok(result);
        }

    }
}
