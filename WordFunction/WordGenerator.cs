using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System.Text;
using Newtonsoft.Json;
using Confluent.Kafka;
using Google.Protobuf;

namespace WordFunction;

public class WordGenerator
{
    private readonly ILogger _logger;
    private readonly IProducer<Null, string> _producer;

    public WordGenerator(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<WordGenerator>();

        var producerconfig = new ProducerConfig
        {
            BootstrapServers = "kafka:29092"
        };

        _producer = new ProducerBuilder<Null, string>(producerconfig).Build();
    }

    [Function("WordGenerator")]
    public void Run(
        [TimerTrigger("0 6 * * *"
            #if DEBUG
                , RunOnStartup=true
            #endif
            )]TimerInfo myTimer)
    {
        _logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

        if (myTimer.ScheduleStatus is not null)
        {
            _logger.LogInformation($"Next timer schedule at: {myTimer.ScheduleStatus.Next}");
        }

        var random = new Random();
        var words = new List<string>() { "speak", "stick", "paste", "tears", "glove" };
        int index = random.Next(words.Count);
        var word = words[index];

        _logger.LogInformation($"Today's word is: {word}");

        /*var RabbitMQServer = "";
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
        
        try
        {
            var factory = new ConnectionFactory()
            { HostName = RabbitMQServer, UserName = RabbitMQUserName, Password = RabbutMQPassword };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                //Direct Exchange Details like name and type of exchange
                channel.ExchangeDeclare("WordExchange", "direct");
                //Declare Queue with Name and a few property related to Queue like durabality of msg, auto delete and many more
                channel.QueueDeclare(queue: "word_queue",
                                     durable: true,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);
                //Bind Queue with Exhange and routing details
                channel.QueueBind(queue: "word_queue", exchange: "WordExchange", routingKey: "word_route");
                //Seriliaze object using Newtonsoft library
                string productDetail = JsonConvert.SerializeObject(word);
                var body = Encoding.UTF8.GetBytes(productDetail);
                var properties = channel.CreateBasicProperties();
                properties.Persistent = true;
                //publish msg
                channel.BasicPublish(exchange: "WordExchange",
                                     routingKey: "word_route",
                                     basicProperties: properties,
                                     body: body);
            }
        }
        catch (Exception)
        {
        }*/

        var kafkamessage = new Message<Null, string> { Value = word, };

        _producer.Produce("word-generator", kafkamessage);
    }
}
