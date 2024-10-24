using GeekShopping.OrderAPI.Model;
using GeekShopping.OrderAPI.Model.Context;
using GeekShopping.OrderAPI.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GeekShopping.OrderAPI.Repository;

public class OrderRepository : IOrderRepository
{
    private readonly DbContextOptions<MySQLContext> _options;

    public OrderRepository(DbContextOptions<MySQLContext> options)
    {
        _options = options;
    }

    public async Task<bool> AddOrder(OrderHeader header)
    {
        if (header is null) return false;

        await using var context = new MySQLContext(_options);

        context.Headers.Add(header);

        await context.SaveChangesAsync();

        return true;
    }

    public async Task UpdateOrderPaymentStatus(long orderHeaderId, bool status)
    {
        await using var context = new MySQLContext(_options);

        var header = await context.Headers.FirstOrDefaultAsync(x => x.Id == orderHeaderId);

        if (header != null)
        {
            header.PaymentStatus = status;

            await context.SaveChangesAsync();
        }

    }
}