using GeekShopping.PaymentAPI.Messages;
using GeekShopping.PaymentAPI.RabbitMQSender.Interface;
using GeekShopping.PaymentProcessor;
using GeekShopping.PaymentProcessor.Interface;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace GeekShopping.PaymentAPI.MessageConsumer
{
    /*
        BackgroundService class provides a convenient way to perform background tasks within the lifecycle of an ASP.NET Core application. 
        This is useful for consuming messages from a RabbitMQ queue continuously without blocking the main flow of the application.
    */
    public class RabbitMQPaymentConsumer : BackgroundService
    {
        private readonly IRabbitMQMessageSender _rabbitMQMessageSender;
        private readonly IProcessPayment _processPayment;
        private readonly IConfiguration _configuration;
        private readonly string _hostName;
        private readonly string _userName;
        private readonly string _password;
        private readonly string _orderPaymentProcessQueueName;
        private readonly string _orderPaymentResultQueueName;
        private readonly IConnection _connection;
        private readonly IModel _channel;

        public RabbitMQPaymentConsumer(
            IRabbitMQMessageSender rabbitMQMessageSender,
            IProcessPayment processPayment,
            IConfiguration configuration)
        {
            _processPayment = processPayment;
            _rabbitMQMessageSender = rabbitMQMessageSender;

            _configuration = configuration;

            _hostName = _configuration["RabbitMQ:HostName"] ?? throw new ArgumentNullException("RabbitMQ HostName is missing");
            _userName = _configuration["RabbitMQ:UserName"] ?? throw new ArgumentNullException("RabbitMQ UserName is missing");
            _password = _configuration["RabbitMQ:Password"] ?? throw new ArgumentNullException("RabbitMQ Password is missing");
            _orderPaymentProcessQueueName = _configuration["RabbitMQ:OrderPaymentProcessQueueName"] ?? throw new ArgumentNullException("RabbitMQ OrderPaymentProcessQueueName is missing");
            _orderPaymentResultQueueName = _configuration["RabbitMQ:OrderPaymentResultQueueName"] ?? throw new ArgumentNullException("RabbitMQ OrderPaymentResultQueueName is missing");

            var factory = new ConnectionFactory
            {
                HostName = _hostName,
                UserName = _userName,
                Password = _password
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.QueueDeclare(queue: _orderPaymentProcessQueueName,
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
                PaymentMessage vo = JsonSerializer.Deserialize<PaymentMessage>(content)!;
                ProcessPayment(vo).GetAwaiter().GetResult();
                _channel.BasicAck(evt.DeliveryTag, false); // Remove the message from the queue
            };
            _channel.BasicConsume(_orderPaymentProcessQueueName, false, consumer); 
            return Task.CompletedTask;
        }

        private async Task ProcessPayment(PaymentMessage vo)
        {
            /*
                In practice, the PaymentProcessor always returns true as it represents the payment processing for an order. 
                This PaymentProcessor could be another microservice, written in a different programming language, 
                or even a job, for example. There are many possibilities.
            */
            var result = _processPayment.PaymentProcessor();

            UpdatePaymentResultMessage paymentResult = new UpdatePaymentResultMessage
            {
                OrderId = vo.OrderId,
                Status = result,
                Email = vo.Email
            };

            try
            {
                _rabbitMQMessageSender.SendMessage(paymentResult, _orderPaymentResultQueueName);
            }
            catch (Exception)
            {
                // TASK: Log exception
                throw;
            }
        }
    }
}
