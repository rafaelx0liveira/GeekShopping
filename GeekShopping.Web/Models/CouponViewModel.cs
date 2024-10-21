namespace GeekShopping.Web.Models;

public class CouponViewModel
{
    public long Id { get; set; }
    public string? Code { get; set; }
    public decimal DiscountAmount { get; set; }
}