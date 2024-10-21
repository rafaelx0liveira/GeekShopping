using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GeekShopping.CartAPI.Model;

[Table("produto")]
public class Product
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
    [Column("id")]
    public long Id { get; set; }

    [Column("nome")]
    [Required]
    [StringLength(150)]
    public string? Name { get; set; }

    [Column("preco")]
    [Required]
    [Range(1, 10000)]
    public decimal Price { get; set; }

    [Column("descricao")]
    [StringLength(500)]
    public string? Description { get; set; }

    [Column("nome_categoria")]
    [StringLength(50)]
    public string? CategoryName { get; set; }

    [Column("image_url")]
    [StringLength(300)]
    public string? ImageUrl { get; set; }
}