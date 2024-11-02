using GeekShopping.Email.Messages;
using GeekShopping.Email.Model;

namespace GeekShopping.Email.Repository.Interfaces;

public interface IEmailRepository
{
    Task SendAndLogEmail(UpdatePaymentResultMessage message);

}