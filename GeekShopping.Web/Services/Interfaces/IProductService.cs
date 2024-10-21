using GeekShopping.Web.Models;

namespace GeekShopping.Web.Services.Interfaces;

public interface IProductService
{
    Task<IEnumerable<ProductViewModel>?> GetAll(string token);
    Task<ProductViewModel?> GetById(long id, string token);
    Task<ProductViewModel?> Create(ProductViewModel productModel, string token);
    Task<ProductViewModel?> Update(ProductViewModel productModel, string token);
    Task<bool> Delete(long id, string token);
}