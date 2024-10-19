using GeekShopping.Web.Models;

namespace GeekShopping.Web.Services.Interfaces;

public interface IProductService
{
    Task<IEnumerable<ProductViewModel>?> ProductGetAll(string token);
    Task<ProductViewModel?> ProductGetById(long id, string token);
    Task<ProductViewModel?> ProductCreate(ProductViewModel productModel, string token);
    Task<ProductViewModel?> ProductUpdate(ProductViewModel productModel, string token);
    Task<bool> ProductDelete(long id, string token);
}