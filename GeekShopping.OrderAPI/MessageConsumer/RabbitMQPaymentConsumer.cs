
using GeekShopping.OrderAPI.Messages;
using GeekShopping.OrderAPI.Model;
using GeekShopping.OrderAPI.RabbitMQSender.Interface;
using GeekShopping.OrderAPI.Repository;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace GeekShopping.OrderAPI.MessageConsumer
{
    /*
        BackgroundService class provides a convenient way to perform background tasks within the lifecycle of an ASP.NET Core application. 
        This is useful for consuming messages from a RabbitMQ queue continuously without blocking the main flow of the application.
    */
    public class RabbitMQPaymentConsumer : BackgroundService
    {
        private readonly OrderRepository _orderRepository;
        private readonly IConfiguration _configuration;
        private readonly string _hostName;
        private readonly string _userName;
        private readonly string _password;
        private readonly string _orderPaymentResultQueueName;
        private IConnection _connection;
        private IModel _channel;

        public RabbitMQPaymentConsumer(
            OrderRepository orderRepository,
            IConfiguration configuration)
        {
            _orderRepository = orderRepository;

            _configuration = configuration;

            _hostName = _configuration["RabbitMQ:HostName"] ?? throw new ArgumentNullException("RabbitMQ HostName is missing");
            _userName = _configuration["RabbitMQ:UserName"] ?? throw new ArgumentNullException("RabbitMQ UserName is missing");
            _password = _configuration["RabbitMQ:Password"] ?? throw new ArgumentNullException("RabbitMQ Password is missing");
            _orderPaymentResultQueueName = _configuration["RabbitMQ:OrderPaymentResultQueueName"] ?? throw new ArgumentNullException("RabbitMQ OrderPaymentResultQueueName is missing");

            var factory = new ConnectionFactory
            {
                HostName = _hostName,
                UserName = _userName,
                Password = _password
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.QueueDeclare(queue: _orderPaymentResultQueueName,
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested(); // Check if the task has been cancelled
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (chanel, evt) =>
            {
                var content = Encoding.UTF8.GetString(evt.Body.ToArray());
                UpdatePaymentResultVO vo = JsonSerializer.Deserialize<UpdatePaymentResultVO>(content)!;
                UpdatePaymentStatus(vo).GetAwaiter().GetResult();
                _channel.BasicAck(evt.DeliveryTag, false); // Remove the message from the queue
            };
            _channel.BasicConsume(_orderPaymentResultQueueName, false, consumer); 
            return Task.CompletedTask;
        }

        private async Task UpdatePaymentStatus(UpdatePaymentResultVO vo)
        {
            
            try
            {
                
                await _orderRepository.UpdateOrderPaymentStatus(vo.OrderId, vo.Status);
            }
            catch (Exception)
            {
                // TASK: Log exception
                throw;
            }
        }
    }
}
