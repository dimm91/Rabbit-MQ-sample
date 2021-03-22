using RabbitMQ.Client;
using System;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMq.Sample.Producer
{
    class Program
    {
        private static readonly string Username = "guest";
        private static readonly string Password = "guest";
        private static readonly string Hostname = "localhost";
        private static readonly string Port = "5672";
        private static readonly string RabbitMqUri = $"amqp://{Username}:{Password}@{Hostname}:{Port}";
        private static readonly string queueName = "first-demo-queue";
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
                    /// https://www.rabbitmq.com/queues.html
                    /// Name: The name of the queue, that will be declared
                    /// Durable: Set if the queue will survive a broker restart
                    /// Exclusive: Set if the queue will be used by one connection and the queue will be deleted when that connection closes
                    /// AutoDelete: Set if the queue will be deleted when its last consumer is cancelled
                    /// Arguments: Optional - used by plugins and broker - specific features such as message TTL, queue length limit, etc)
                    ///</summary>
                    channel.QueueDeclare(queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);

                    ///Anonymous Type
                    var message = new { Name = index, Message = "This message is from the user " + index };
                    var body = Encoding.UTF8.GetBytes(System.Text.Json.JsonSerializer.Serialize(message));

                    channel.BasicPublish(string.Empty, queueName, null, body);
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
