using GeekShopping.Web.Models;

namespace GeekShopping.Web.Services.Interfaces;

public interface IProductService
{
    Task<IEnumerable<ProductModel>?> ProductGetAll(string token);
    Task<ProductModel?> ProductGetById(long id, string token);
    Task<ProductModel?> ProductCreate(ProductModel productModel, string token);
    Task<ProductModel?> ProductUpdate(ProductModel productModel, string token);
    Task<bool> ProductDelete(long id, string token);
}