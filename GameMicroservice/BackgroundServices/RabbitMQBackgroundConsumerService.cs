using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System.Text;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using Confluent.Kafka;

namespace GameMicroservice.BackgroundServices;

public class RabbitMQBackgroundConsumerService : BackgroundService
{
    private IConnection _connection;
    private IModel _channel;
    private IServiceScopeFactory serviceScopeFactory;
    private readonly ILogger _logger;
    private readonly IConsumer<Ignore, string> _consumer;
    public RabbitMQBackgroundConsumerService(IServiceScopeFactory _serviceScopeFactory, ILoggerFactory loggerFactory)
    {
        serviceScopeFactory = _serviceScopeFactory;
        _logger = loggerFactory.CreateLogger<RabbitMQBackgroundConsumerService>();
        //InitRabbitMQ();

        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = "localhost:29092",
            GroupId = "InventoryConsumerGroup",
            AutoOffsetReset = AutoOffsetReset.Earliest
        };

        _consumer = new ConsumerBuilder<Ignore, string>(consumerConfig).Build();
    }
    private void InitRabbitMQ()
    {
        _logger.LogInformation("Initializing RabbitMQ");
        var RabbitMQServer = "";
        if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production")
        {
            RabbitMQServer = "192.168.2.1";
        }
        else
        {
            RabbitMQServer = "localhost";
        }
        var RabbitMQUserName = "guest";
        var RabbutMQPassword = "guest";

        var factory = new ConnectionFactory();
        factory.Uri = new Uri("amqp://guest:guest@localhost:15672/");
        factory.UserName = "guest";
        factory.Password = "guest";
        factory.VirtualHost = "/";
        factory.HostName = "localhost";
        factory.Port = AmqpTcpEndpoint.UseDefaultPort;
        factory.Ssl = new SslOption
        {
            ServerName = "localhost",
            Enabled = true
        };
        factory.RequestedHeartbeat = TimeSpan.FromSeconds(60);
        // create connection
        _connection = factory.CreateConnection();
        // create channel
        _channel = _connection.CreateModel();
        //Direct Exchange Details like name and type of exchange
        _channel.ExchangeDeclare("WordExchange", "direct");
        //Declare Queue with Name and a few property related to Queue like durabality of msg, auto delete and many more
        _channel.QueueDeclare(queue: "word_queue",
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

        _channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);
        _channel.QueueBind(queue: "word_queue", exchange: "WordExchange", routingKey: "word_route");
        _connection.ConnectionShutdown += RabbitMQ_ConnectionShutdown;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        /*stoppingToken.ThrowIfCancellationRequested();
        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += (ch, ea) =>
        {
            // received message
            var content = System.Text.Encoding.UTF8.GetString(ea.Body.ToArray());
            // acknowledge the received message
            _channel.BasicAck(ea.DeliveryTag, false);
            //Deserilized Message
            var message = Encoding.UTF8.GetString(ea.Body.ToArray());
            var productDetails = JsonConvert.DeserializeObject<string>(message);
            _logger.LogInformation($"Received the word: {productDetails}");
            //Stored Offer Details into the Database
            /*using (var scope = serviceScopeFactory.CreateScope())
            {
                var _dbContext = scope.ServiceProvider.GetRequiredService<DbContextClass>();
                var result = _dbContext.ProductOffers.Add(productDetails);
                _dbContext.SaveChanges();
            }
        };
        consumer.Shutdown += OnConsumerShutdown;
        consumer.Registered += OnConsumerRegistered;
        consumer.Unregistered += OnConsumerUnregistered;
        consumer.ConsumerCancelled += OnConsumerConsumerCancelled;
        _channel.BasicConsume("word_queue", false, consumer);
        return Task.CompletedTask;*/

        _consumer.Subscribe("word-generator");

        while (!stoppingToken.IsCancellationRequested)
        {
            ProcessKafkaMessage(stoppingToken);

            Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }

        _consumer.Close();
    }

    public void ProcessKafkaMessage(CancellationToken stoppingToken)
    {
        try
        {
            var consumeResult = _consumer.Consume(stoppingToken);

            var message = consumeResult.Message.Value;

            _logger.LogInformation($"Received inventory update: {message}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error processing Kafka message: {ex.Message}");
        }
    }

    private void OnConsumerConsumerCancelled(object sender, ConsumerEventArgs e) { }
    private void OnConsumerUnregistered(object sender, ConsumerEventArgs e) { }
    private void OnConsumerRegistered(object sender, ConsumerEventArgs e) { }
    private void OnConsumerShutdown(object sender, ShutdownEventArgs e) { }
    private void RabbitMQ_ConnectionShutdown(object sender, ShutdownEventArgs e) { }
    public override void Dispose()
    {
        _channel.Close();
        _connection.Close();
        base.Dispose();
    }
}
