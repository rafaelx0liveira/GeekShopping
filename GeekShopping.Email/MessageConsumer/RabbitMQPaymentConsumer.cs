using GeekShopping.Email.Messages;
using GeekShopping.Email.Repository;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace GeekShopping.Email.MessageConsumer
{
    /*
        BackgroundService class provides a convenient way to perform background tasks within the lifecycle of an ASP.NET Core application. 
        This is useful for consuming messages from a RabbitMQ queue continuously without blocking the main flow of the application.
    */
    public class RabbitMQPaymentConsumer : BackgroundService
    {
        private readonly EmailRepository _emailRepository;
        private readonly IConfiguration _configuration;
        private readonly string _hostName;
        private readonly string _userName;
        private readonly string _password;
        private readonly string _orderPaymentResultQueueName;
        private readonly string _exchangeName;
        string _queueName;
        private IConnection _connection;
        private IModel _channel;

        public RabbitMQPaymentConsumer(
            EmailRepository orderRepository,
            IConfiguration configuration)
        {
            _emailRepository = orderRepository;

            _configuration = configuration;

            _hostName = _configuration["RabbitMQ:HostName"] ?? throw new ArgumentNullException("RabbitMQ HostName is missing");
            _userName = _configuration["RabbitMQ:UserName"] ?? throw new ArgumentNullException("RabbitMQ UserName is missing");
            _password = _configuration["RabbitMQ:Password"] ?? throw new ArgumentNullException("RabbitMQ Password is missing");
            _orderPaymentResultQueueName = _configuration["RabbitMQ:OrderPaymentResultQueueName"] ?? throw new ArgumentNullException("RabbitMQ OrderPaymentResultQueueName is missing");
            _exchangeName = _configuration["RabbitMQ:ExchangeFanoutName"] ?? throw new ArgumentNullException("RabbitMQ ExchangeFanoutName is missing");

            var factory = new ConnectionFactory
            {
                HostName = _hostName,
                UserName = _userName,
                Password = _password
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.ExchangeDeclare(
                exchange: _exchangeName,
                type: ExchangeType.Fanout
            );

            _queueName = _channel.QueueDeclare().QueueName; // Create a queue with a random name and return the name

            _channel.QueueBind(
                queue: _queueName,
                exchange: _exchangeName,
                routingKey: ""
            );

        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested(); // Check if the task has been cancelled
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (chanel, evt) =>
            {
                var content = Encoding.UTF8.GetString(evt.Body.ToArray());
                UpdatePaymentResultMessage message = JsonSerializer.Deserialize<UpdatePaymentResultMessage>(content)!;
                ProcessLogs(message).GetAwaiter().GetResult();
                _channel.BasicAck(evt.DeliveryTag, false); // Remove the message from the queue
            };
            _channel.BasicConsume(_queueName, false, consumer); 
            return Task.CompletedTask;
        }

        private async Task ProcessLogs(UpdatePaymentResultMessage message)
        {
            try
            {
                await _emailRepository.SendAndLogEmail(message);
            }
            catch (Exception)
            {
                // TASK: Log exception
                throw;
            }
        }
    }
}
