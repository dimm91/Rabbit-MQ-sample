using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;

namespace RabbitMq.Sample.Consumer
{
    class Program
    {
        private static readonly string Username = "guest";
        private static readonly string Password = "guest";
        private static readonly string Hostname = "localhost";
        private static readonly string Port = "5672";
        private static readonly string RabbitMqUri = $"amqp://{Username}:{Password}@{Hostname}:{Port}";
        private static readonly string queueName = "first-demo-queue";
        static void Main(string[] args)
        {
            try
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

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (sender, e) =>
                {
                    var bodyArray = e.Body.ToArray();//Byte array of message
                    var message = Encoding.UTF8.GetString(bodyArray);

                    Console.WriteLine($"[{DateTime.UtcNow}] - Message received: " + message);
                };

                ///<summary>
                /// https://www.rabbitmq.com/confirms.html#acknowledgement-modes
                /// AutoAck: set if a message is considered to be successfully delivered immediately after it is sent
                ///</summary>
                channel.BasicConsume(queueName, true, consumer);
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadLine();
            }
        }
    }
}
