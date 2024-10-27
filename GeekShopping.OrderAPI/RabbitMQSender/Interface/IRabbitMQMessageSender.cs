using GeekShopping.MessageBus;

namespace GeekShopping.OrderAPI.RabbitMQSender.Interface
{
    public interface IRabbitMQMessageSender
    {
        void SendMessage(BaseMessage baseMessage, string queueName);
    }
}
