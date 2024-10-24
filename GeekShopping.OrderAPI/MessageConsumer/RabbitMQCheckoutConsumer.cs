
using GeekShopping.OrderAPI.Messages;
using GeekShopping.OrderAPI.Model;
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
    public class RabbitMQCheckoutConsumer : BackgroundService
    {
        private readonly OrderRepository _orderRepository;
        private readonly IConfiguration _configuration;
        private readonly string _hostName;
        private readonly string _userName;
        private readonly string _password;
        private readonly string _queueName;
        private IConnection _connection;
        private IModel _channel;

        public RabbitMQCheckoutConsumer(
            OrderRepository orderRepository,
            IConfiguration configuration)
        {
            _orderRepository = orderRepository;

            _configuration = configuration;

            _hostName = _configuration["RabbitMQ:HostName"] ?? throw new ArgumentNullException("RabbitMQ HostName is missing");
            _userName = _configuration["RabbitMQ:UserName"] ?? throw new ArgumentNullException("RabbitMQ UserName is missing");
            _password = _configuration["RabbitMQ:Password"] ?? throw new ArgumentNullException("RabbitMQ Password is missing");
            _queueName = _configuration["RabbitMQ:QueueName"] ?? throw new ArgumentNullException("RabbitMQ QueueName is missing");

            var factory = new ConnectionFactory
            {
                HostName = _hostName,
                UserName = _userName,
                Password = _password
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.QueueDeclare(queue: _queueName,
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
                CheckoutHeaderVO vo = JsonSerializer.Deserialize<CheckoutHeaderVO>(content)!;
                ProcessOrder(vo).GetAwaiter().GetResult();
                _channel.BasicAck(evt.DeliveryTag, false); // Remove the message from the queue
            };
            _channel.BasicConsume(_queueName, false, consumer); 
            return Task.CompletedTask;
        }

        private async Task ProcessOrder(CheckoutHeaderVO vo)
        {
            OrderHeader order = new()
            {
                UserId = vo.UserId,
                FirstName = vo.FirstName,
                LastName = vo.LastName,
                OrderDetails = [],
                CardNumber = vo.CardNumber,
                CouponCode = vo.CouponCode,
                CVV = vo.CVV,
                DiscountAmount = vo.DiscountAmount,
                Email = vo.Email,
                ExpiryMonthYear = vo.ExpiryMothYear,
                OrderTime = DateTime.Now,
                PurchaseAmount = vo.PurchaseAmount,
                PaymentStatus = false,
                Phone = vo.Phone,
                DateTime = vo.DateTime
            };

            foreach (var details in vo.ListCartDetail!)
            {
                OrderDetail detail = new()
                {
                    ProductId = details.ProductId,
                    ProductName = details.Product!.Name!,
                    Price = details.Product.Price,
                    Count = details.Count,
                };
                order.CartTotalItens += details.Count;
                order.OrderDetails.Add(detail);
            }

            await _orderRepository.AddOrder(order);
        }
    }
}
