using GeekShopping.MessageBus;

namespace GeekShopping.CartAPI.RabbitMQSender.Interface
{
    public interface IRabbitMQMessageSender
    {
        void SendMessage(BaseMessage baseMessage, string queueName);
    }
}
