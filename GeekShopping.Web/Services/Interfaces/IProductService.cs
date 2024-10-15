using GeekShopping.Web.Models;

namespace GeekShopping.Web.Services.Interfaces;

public interface IProductService
{
    Task<IEnumerable<ProductModel>?> GetAll(string token);
    Task<ProductModel?> GetById(long id, string token);
    Task<ProductModel?> Create(ProductModel productModel, string token);
    Task<ProductModel?> Update(ProductModel productModel, string token);
    Task<bool> Delete(long id, string token);
}