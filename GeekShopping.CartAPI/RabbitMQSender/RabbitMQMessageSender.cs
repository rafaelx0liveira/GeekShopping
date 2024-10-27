using GeekShopping.CartAPI.Messages;
using GeekShopping.CartAPI.RabbitMQSender.Interface;
using GeekShopping.MessageBus;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace GeekShopping.CartAPI.RabbitMQSender
{
    public class RabbitMQMessageSender : IRabbitMQMessageSender
    {
        private readonly IConfiguration _configuration;
        private readonly string _hostName;
        private readonly string _userName;
        private readonly string _password;
        private IConnection _connection;


        public RabbitMQMessageSender(IConfiguration configuration)
        {

            _configuration = configuration;

            _hostName = _configuration["RabbitMQ:HostName"] ?? throw new ArgumentNullException("RabbitMQ HostName is missing");
            _userName = _configuration["RabbitMQ:UserName"] ?? throw new ArgumentNullException("RabbitMQ UserName is missing");
            _password = _configuration["RabbitMQ:Password"] ?? throw new ArgumentNullException("RabbitMQ Password is missing");
        }

        public void SendMessage(BaseMessage baseMessage, string queueName)
        {

            if (ConnectionExists())
            {
                using var channel = _connection.CreateModel();
                channel.QueueDeclare(queue: queueName,
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                byte[] body = GetMessageAsByteArray(baseMessage);

                channel.BasicPublish(exchange: "",
                                     routingKey: queueName,
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

            var json = JsonSerializer.Serialize<CheckoutHeaderVO>((CheckoutHeaderVO)baseMessage, options);

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
