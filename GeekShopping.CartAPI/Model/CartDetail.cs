using GeekShopping.CartAPI.Model.Base;
using System.ComponentModel.DataAnnotations.Schema;

namespace GeekShopping.CartAPI.Model;

[Table("carrinho_detalhe")]
public class CartDetail : BaseEntity
{
    [Column("id_carrinho_cabecalho")]
    public long CartHeaderId { get; set; }

    [ForeignKey("CartHeaderId")]
    public virtual CartHeader? CartHeader { get; set; }

    [Column("id_produto")]
    public long ProductId { get; set; }

    [ForeignKey("ProductId")]
    public virtual Product? Product { get; set; }

    [Column("count")]
    public int Count { get; set; }
}