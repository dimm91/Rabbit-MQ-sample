using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;

namespace Sample.Publish.Subscriber.PrintLog
{
    class Program
    {
        private static readonly string Username = "guest";
        private static readonly string Password = "guest";
        private static readonly string Hostname = "localhost";
        private static readonly string Port = "5672";
        private static readonly string RabbitMqUri = $"amqp://{Username}:{Password}@{Hostname}:{Port}";
        private static readonly string QueueName = "log-print-demo-queue";
        private static readonly string ExchangeName = "logs";

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

                channel.ExchangeDeclare(exchange: ExchangeName , type: ExchangeType.Fanout);
                channel.QueueDeclare(QueueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
                /// We bind the Queue to the Exchanged declared before
                /// The Routing Key is empty since for the exchange of type "Fanout" this parameter is ignored.
                channel.QueueBind(QueueName, ExchangeName, string.Empty);

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (sender, e) =>
                {
                    var bodyArray = e.Body.ToArray();//Byte array of message
                    var message = Encoding.UTF8.GetString(bodyArray);

                    Console.WriteLine($"[{DateTime.UtcNow}] - Message received: " + message);
                };

                channel.BasicConsume(QueueName, true, consumer);
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
