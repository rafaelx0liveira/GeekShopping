using System.ComponentModel.DataAnnotations.Schema;
using GeekShopping.CartAPI.Model.Base;

namespace GeekShopping.CartAPI.Model;

[Table("carrinho_cabecalho")]
public class CartHeader : BaseEntity
{
    [Column("id_usuario")]
    public string? UserId { get; set; }

    [Column("codigo_cupom")]
    public string? CouponCode { get; set; }
}