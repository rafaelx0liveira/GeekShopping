using AutoMapper;
using GeekShopping.ProductAPI.Data.ValueObjects;
using GeekShopping.ProductAPI.Model;
using GeekShopping.ProductAPI.Model.Context;
using GeekShopping.ProductAPI.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GeekShopping.ProductAPI.Repository;

public class ProductRepository(MySQLContext context, IMapper mapper) : IProductRepository
{
    private readonly MySQLContext _context = context;
    private readonly IMapper _mapper = mapper;

    public async Task<IEnumerable<ProductVO>> GetAll()
    {
        List<Product> products = await _context.Products.ToListAsync();
        return _mapper.Map<List<ProductVO>>(products);
    }

    public async Task<ProductVO> GetById(long id)
    {
        Product product = await _context.Products.Where(x => x.Id == id).FirstOrDefaultAsync() ?? new Product();
        return _mapper.Map<ProductVO>(product);
    }

    public async Task<ProductVO> Create(ProductVO productVO)
    {
        Product product = _mapper.Map<Product>(productVO);
        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        return _mapper.Map<ProductVO>(product);
    }

    public async Task<ProductVO> Update(ProductVO productVO)
    {
        Product product = _mapper.Map<Product>(productVO);
        _context.Products.Update(product);
        await _context.SaveChangesAsync();
        return _mapper.Map<ProductVO>(product);
    }

    public async Task<bool> Delete(long id)
    {
        try
        {
            Product product = await _context.Products.Where(x => x.Id == id).FirstOrDefaultAsync() ?? new Product();
            if (product.Id <= 0) return false;
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}