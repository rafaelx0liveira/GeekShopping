using GeekShopping.MessageBus;

namespace GeekShopping.PaymentAPI.RabbitMQSender.Interface
{
    public interface IRabbitMQMessageSender
    {
        void SendMessage(BaseMessage baseMessage);
    }
}
