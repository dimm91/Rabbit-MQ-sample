using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sample.Work.Queue.Consumer
{
    class Program
    {
        private static readonly string Username = "guest";
        private static readonly string Password = "guest";
        private static readonly string Hostname = "localhost";
        private static readonly string Port = "5672";
        private static readonly string RabbitMqUri = $"amqp://{Username}:{Password}@{Hostname}:{Port}";
        private static readonly string queueName = "work-queue";
        static async Task Main(string[] args)
        {
            try
            {
                var factory = new ConnectionFactory
                {
                    Uri = new Uri(RabbitMqUri)
                };

                using var connection = factory.CreateConnection();
                using var channel = connection.CreateModel();

                channel.QueueDeclare(queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
                
                // Tells RabbitMQ not to give more than one message to a worker at a time
                channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (sender, e) =>
                {
                    var bodyArray = e.Body.ToArray();//Byte array of message
                    var message = Encoding.UTF8.GetString(bodyArray);

                    Console.WriteLine($"[{DateTime.UtcNow}] - Message received: " + message);
                    Thread.Sleep(5000);
                    // Manually acknowledge that the message was received and procesed correctly.
                    channel.BasicAck(deliveryTag: e.DeliveryTag, multiple: false);
                };

                // https://www.rabbitmq.com/tutorials/tutorial-two-dotnet.html
                // We set the 'autoAck' to false, since we will to the acknowledge of the message manually
                channel.BasicConsume(queueName, false, consumer);
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
