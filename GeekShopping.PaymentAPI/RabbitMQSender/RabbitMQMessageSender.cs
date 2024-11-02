using GeekShopping.PaymentAPI.Messages;
using GeekShopping.PaymentAPI.RabbitMQSender.Interface;
using GeekShopping.MessageBus;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace GeekShopping.PaymentAPI.RabbitMQSender
{
    public class RabbitMQMessageSender : IRabbitMQMessageSender
    {
        private readonly IConfiguration _configuration;
        private readonly string _hostName;
        private readonly string _userName;
        private readonly string _password;
        private IConnection _connection;
        private readonly string _fanoutExchangeName;
        private readonly string _directExchangeName;
        private readonly string _paymentEmailUpdateQueueName;
        private readonly string _paymentOrderUpdateQueueName;


        public RabbitMQMessageSender(IConfiguration configuration)
        {

            _configuration = configuration;

            _hostName = _configuration["RabbitMQ:HostName"] ?? throw new ArgumentNullException("RabbitMQ HostName is missing");
            _userName = _configuration["RabbitMQ:UserName"] ?? throw new ArgumentNullException("RabbitMQ UserName is missing");
            _password = _configuration["RabbitMQ:Password"] ?? throw new ArgumentNullException("RabbitMQ Password is missing");
            _fanoutExchangeName = _configuration["RabbitMQ:ExchangeFanoutName"] ?? throw new ArgumentNullException("RabbitMQ ExchangeFanoutName is missing");
            _directExchangeName = _configuration["RabbitMQ:ExchangeDirectName"] ?? throw new ArgumentNullException("RabbitMQ ExchangeDirectName is missing");
            _paymentEmailUpdateQueueName = _configuration["RabbitMQ:PaymentEmailUpdateQueueName"] ?? throw new ArgumentNullException("RabbitMQ PaymentEmailUpdateQueueName is missing");
            _paymentOrderUpdateQueueName = _configuration["RabbitMQ:PaymentOrderUpdateQueueName"] ?? throw new ArgumentNullException("RabbitMQ PaymentOrderUpdateQueueName is missing");
        }

        public void SendMessage(BaseMessage baseMessage)
        {

            if (ConnectionExists())
            {
                using var channel = _connection.CreateModel();

                // Declare the exchange fanout
                //channel.ExchangeDeclare(
                //    exchange: _fanoutExchangeName, 
                //    type: ExchangeType.Fanout,
                //    durable: false // Durable false means that the exchange will not be recreated when the RabbitMQ server restarts
                //    );

                // Declare the exchange direct
                channel.ExchangeDeclare(
                    exchange: _directExchangeName,
                    type: ExchangeType.Direct,
                    durable: false // Durable false means that the exchange will not be recreated when the RabbitMQ server restarts
                    );

                // Declare the queue for the payment email update
                channel.QueueDeclare(
                    queue: _paymentEmailUpdateQueueName,
                    durable: false,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null
                );

                // Declare the queue for the payment order update
                channel.QueueDeclare(
                    queue: _paymentOrderUpdateQueueName,
                    durable: false,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null
                );

                // Bind the queue for the payment email update
                channel.QueueBind(
                    queue: _paymentEmailUpdateQueueName,
                    exchange: _directExchangeName,
                    routingKey: "PaymentEmail"
                );

                // Bind the queue for the payment order update
                channel.QueueBind(
                    queue: _paymentOrderUpdateQueueName,
                    exchange: _directExchangeName,
                    routingKey: "PaymentOrder"
                );

                byte[] body = GetMessageAsByteArray(baseMessage);

                // Publish the message to the exchange fanout
                //channel.BasicPublish(exchange: _fanoutExchangeName,
                //                     routingKey: "",
                //                     basicProperties: null,
                //                     body: body);

                // Publish the message to the exchange direct for the payment email update
                channel.BasicPublish(exchange: _directExchangeName,
                                     routingKey: "PaymentEmail",
                                     basicProperties: null,
                                     body: body);

                // Publish the message to the exchange direct for the payment order update
                channel.BasicPublish(exchange: _directExchangeName,
                                     routingKey: "PaymentOrder",
                                     basicProperties: null,
                                     body: body);
            }
        }

        private byte[] GetMessageAsByteArray(BaseMessage baseMessage)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            var json = JsonSerializer.Serialize<UpdatePaymentResultMessage>((UpdatePaymentResultMessage)baseMessage, options);

            var body = Encoding.UTF8.GetBytes(json);

            return body;
        }

        private void CreateConnection()
        {
            try
            {
                var factory = new ConnectionFactory
                {
                    HostName = _hostName,
                    UserName = _userName,
                    Password = _password
                };

                _connection = factory.CreateConnection();
            }
            catch (Exception)
            {
                // TASK: Log the exception
                throw;
            }
        }

        private bool ConnectionExists()
        {
            if (_connection != null) return true;

            CreateConnection();

            return _connection != null;
        }
    }
}
