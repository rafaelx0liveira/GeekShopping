using GeekShopping.CouponAPI.Model.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GeekShopping.CouponAPI.Model;

[Table("cupom")]
public class Coupon : BaseEntity
{
    [Column("codigo")]
    [Required]
    [StringLength(30)]
    public string? Code { get; set; }

    [Column("valor_desconto")]
    [Required]
    public decimal DiscountAmount { get; set; }
}