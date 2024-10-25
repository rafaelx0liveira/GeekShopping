using GeekShopping.CartAPI.Data.ValueObjects;
using GeekShopping.CartAPI.Messages;
using GeekShopping.CartAPI.RabbitMQSender.Interface;
using GeekShopping.CartAPI.Repository.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GeekShopping.CartAPI.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class CartController: ControllerBase
{
    private readonly ICartRepository _cartRepository;
    private readonly ICouponRepository _couponRepository;
    private readonly IRabbitMQMessageSender _rabbitMQMessageSender;
    private readonly IConfiguration _configuration;
    private readonly string _queueName;

    public CartController(ICartRepository cartRepository, 
        IRabbitMQMessageSender rabbitMQMessageSender,
        IConfiguration configuration,
        ICouponRepository couponRepository)
    {
        _cartRepository = cartRepository;
        _couponRepository = couponRepository;
        _rabbitMQMessageSender = rabbitMQMessageSender;
        _configuration = configuration;

        _queueName = _configuration["RabbitMQ:QueueName"] ?? throw new ArgumentNullException("RabbitMQ QueueName is missing");
    }

    [HttpGet("GetByUserId/{userId}")]
    public async Task<ActionResult<CartVO>> GetByUserId(string userId)
    {
        var cart = await _cartRepository.GetByUserId(userId);
        if (cart == null) return NotFound();
        return Ok(cart);
    }

    [HttpPost("AddItem")]
    public async Task<ActionResult<CartVO>> AddItem(CartVO vo)
    {
        var cart = await _cartRepository.SaveOrUpdate(vo);
        if (cart == null) return NotFound();
        return Ok(cart);
    }

    [HttpPut("Update")]
    public async Task<ActionResult<CartVO>> Update(CartVO vo)
    {
        var cart = await _cartRepository.SaveOrUpdate(vo);
        if (cart == null) return NotFound();
        return Ok(cart);
    }

    [HttpDelete("RemoveItem/{id}")]
    public async Task<ActionResult<CartVO>> RemoveItem(int id)
    {
        var status = await _cartRepository.RemoveItem(id);
        if (!status) return BadRequest();
        return Ok(status);
    }

    [HttpPost("ApplyCoupon")]
    public async Task<ActionResult<CartVO>> ApplyCoupon(CartVO vo)
    {
        var status = await _cartRepository.ApplyCoupon(vo.CartHeader!.UserId!, vo.CartHeader.CouponCode!);
        if (!status) return NotFound();
        return Ok(status);
    }

    [HttpDelete("RemoveCoupon/{userId}")]
    public async Task<ActionResult<CartVO>> RemoveCoupon(string userId)
    {
        var status = await _cartRepository.RemoveCoupon(userId);
        if (!status) return NotFound();
        return Ok(status);
    }

    [HttpPost("checkout")]
    public async Task<ActionResult<CheckoutHeaderVO>> Checkout(CheckoutHeaderVO vo)
    {
        string token = Request.Headers["Authorization"]!;

        if (vo?.UserId == null) return BadRequest("UserId is required!");

        var cart = await _cartRepository.GetByUserId(vo.UserId!);
        if (cart == null) return NotFound();

        if (!string.IsNullOrEmpty(vo.CouponCode))
        {
            CouponVO? coupon = await _couponRepository.GetByCode(vo.CouponCode, token.Replace("Bearer ", ""));

            if (vo.DiscountAmount != coupon!.DiscountAmount)
            {
                return StatusCode(412); // Precondition Failed, the coupon discount amount is different from the cart discount amount
            }
        }

        vo.ListCartDetail = cart.ListCartDetail;
        vo.DateTime = DateTime.Now;

        // RabbitMQ logic
        _rabbitMQMessageSender.SendMessage(vo, _queueName);

        return Ok(vo);
    }
}