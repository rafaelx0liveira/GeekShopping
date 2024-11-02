using GeekShopping.Email.Messages;
using GeekShopping.Email.Model;
using GeekShopping.Email.Model.Context;
using GeekShopping.Email.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GeekShopping.Email.Repository;

public class EmailRepository : IEmailRepository
{
    private readonly DbContextOptions<MySQLContext> _options;

    public EmailRepository(DbContextOptions<MySQLContext> options)
    {
        _options = options;
    }

    public async Task SendAndLogEmail(UpdatePaymentResultMessage message)
    {
        EmailLog email = new EmailLog
        {
            Email = message.Email,
            SentDate = DateTime.Now,
            Log = $"Order - {message.OrderId} has been created successfully."
        };

        await using var _dbContext = new MySQLContext(_options);

        _dbContext.Emails.Add(email);

        await _dbContext.SaveChangesAsync();

    }

}
