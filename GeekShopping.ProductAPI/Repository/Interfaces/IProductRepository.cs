using GeekShopping.ProductAPI.Data.ValueObjects;

namespace GeekShopping.ProductAPI.Repository.Interfaces;

public interface IProductRepository
{
    Task<IEnumerable<ProductVO>> GetAll();
    Task<ProductVO> GetById(long id);
    Task<ProductVO> Create(ProductVO productVO);
    Task<ProductVO> Update(ProductVO productVO);
    Task<bool> Delete(long id);
}