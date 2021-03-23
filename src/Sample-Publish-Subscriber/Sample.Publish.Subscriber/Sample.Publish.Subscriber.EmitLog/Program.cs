using RabbitMQ.Client;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Sample.Publish.Subscriber.EmitLog
{
    class Program
    {
        private static readonly string Username = "guest";
        private static readonly string Password = "guest";
        private static readonly string Hostname = "localhost";
        private static readonly string Port = "5672";
        private static readonly string RabbitMqUri = $"amqp://{Username}:{Password}@{Hostname}:{Port}";
        private static readonly string QueueName = "log-demo-queue";
        private static readonly string ExchangeName = "logs";

        static async Task Main(string[] args)
        {
            try
            {
                var index = 0;
                while (true)
                {

                    var factory = new ConnectionFactory
                    {
                        Uri = new Uri(RabbitMqUri)
                    };

                    using var connection = factory.CreateConnection();
                    using var channel = connection.CreateModel();
                    ///<summary>
                    /// https://www.rabbitmq.com/tutorials/amqp-concepts.html#exchanges
                    /// Type of Exchanges:
                    /// * Direct: A direct exchange delivers messages to queues based on the message routing key
                    /// * Fanout: A fanout exchange routes messages to all of the queues that are bound to it and the routing key is ignored
                    /// * Headers: A headers exchange is designed for routing on multiple attributes that are more easily expressed as message headers than a routing key. Headers exchanges ignore the routing key attribute.
                    /// * Topic: Topic exchanges route messages to one or many queues based on matching between a message routing key and the pattern that was used to bind a queue to an exchange
                    ///</summary>
                    channel.ExchangeDeclare(exchange: ExchangeName, type: ExchangeType.Fanout);

                    ///Anonymous Type
                    var message = new { Date = DateTime.UtcNow, Message = "This is the message log with index: " + index };
                    var body = Encoding.UTF8.GetBytes(System.Text.Json.JsonSerializer.Serialize(message));

                    channel.BasicPublish(ExchangeName, string.Empty, null, body);
                    index++;

                    await Task.Delay(2000);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadLine();
            }
        }
    }
}
