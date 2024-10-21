namespace GeekShopping.Web.Models;

public class CartViewModel
{
    public CartHeaderViewModel? CartHeader { get; set; }
    public IEnumerable<CartDetailViewModel>? ListCartDetail { get; set; }
}