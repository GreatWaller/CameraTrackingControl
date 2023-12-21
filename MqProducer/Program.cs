// See https://aka.ms/new-console-template for more information

using System.Text;
using RabbitMQ.Client;
Console.WriteLine("Hello, Mq!");

var factory = new ConnectionFactory { HostName = "192.168.1.200", UserName = "cloudsense", Password = "asdf1234" };
using var connection = factory.CreateConnection();
using var channel = connection.CreateModel();

channel.QueueDeclare(queue: "logs",
                     durable: false,
                     exclusive: false,
                     autoDelete: false,
                     arguments: null);

const string message = "0 0.8 0.6 0.7 0.2 0.2 ";
var body = Encoding.UTF8.GetBytes(message);

for (int i = 0; i < 10; i++)
{
    channel.BasicPublish(exchange: string.Empty,
                     routingKey: "logs",
                     basicProperties: null,
                     body: body);
    Thread.Sleep(30);
}

Console.WriteLine($" [x] Sent {message}");

Console.WriteLine(" Press [enter] to exit.");
Console.ReadLine();

