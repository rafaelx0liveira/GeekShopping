
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
        private readonly string _exchangeName; 
        private readonly string _directExchangeName;
        private readonly string _paymentEmailUpdateQueueName;
        private readonly string _paymentOrderUpdateQueueName;
        //string _queueName; // The name of the queue for exchange fanout
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
            _exchangeName = _configuration["RabbitMQ:ExchangeFanoutName"] ?? throw new ArgumentNullException("RabbitMQ ExchangeFanoutName is missing");

            _directExchangeName = _configuration["RabbitMQ:ExchangeDirectName"] ?? throw new ArgumentNullException("RabbitMQ ExchangeDirectName is missing");

            _paymentEmailUpdateQueueName = _configuration["RabbitMQ:PaymentEmailUpdateQueueName"] ?? throw new ArgumentNullException("RabbitMQ PaymentEmailUpdateQueueName is missing");
            _paymentOrderUpdateQueueName = _configuration["RabbitMQ:PaymentOrderUpdateQueueName"] ?? throw new ArgumentNullException("RabbitMQ PaymentOrderUpdateQueueName is missing");

            var factory = new ConnectionFactory
            {
                HostName = _hostName,
                UserName = _userName,
                Password = _password
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            // Declare the exchange fanout
            //_channel.ExchangeDeclare(
            //    exchange: _exchangeName,
            //    type: ExchangeType.Fanout
            //);

            //_queueName = _channel.QueueDeclare().QueueName; // Create a queue with a random name and return the name

            //_channel.QueueBind(
            //    queue: _queueName,
            //    exchange: _exchangeName,
            //    routingKey: ""
            //);

            // Declare the exchange direct
            _channel.ExchangeDeclare(
                exchange: _directExchangeName,
                type: ExchangeType.Direct
            );

            // Declare the queue for the payment email update
            _channel.QueueDeclare(
                queue: _paymentOrderUpdateQueueName,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );

            // Bind the queue for the payment email update
            _channel.QueueBind(
                queue: _paymentOrderUpdateQueueName,
                exchange: _directExchangeName,
                routingKey: "PaymentOrder"
            );

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
            _channel.BasicConsume(_paymentOrderUpdateQueueName, false, consumer); 
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
