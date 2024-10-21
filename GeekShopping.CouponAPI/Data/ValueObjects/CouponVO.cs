namespace GeekShopping.CouponAPI.Data.ValueObjects;

public class CouponVO
{
    public long Id { get; set; }
    public string? Code { get; set; }
    public decimal DiscountAmount { get; set; }
}