using CameraTrackingControl;
using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace CameraTrackingControl
{


    internal delegate void ReceivedEventHandler(ReceivedEventArgs e);
    internal class MqConsumer
    {
        private string hostName = "192.168.1.200";
        private string userName = "cloudsense";
        private string password = "asdf1234";
        private string exchange = "";
        private string queue = "logs";

        private ConnectionFactory factory;

        public event ReceivedEventHandler Received;

        protected virtual void OnReceived(ReceivedEventArgs e)
        {
            if (Received != null)
            {
                Received(e);
            }
        }

        public MqConsumer(string queueName ="logs")
        {
            queue =queueName;
            factory = new ConnectionFactory { HostName = hostName, UserName = userName, Password = password };
        }

        public void Consume()
        {
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                //channel.ExchangeDeclare(exchange: exchange, type: ExchangeType.Fanout, durable: false, autoDelete: false);

                channel.QueueDeclare(queue: queue, durable: false, exclusive: false, arguments: null, autoDelete: false);

                // Random Queue Name, this way each consumer gets it's own queue, what about durability for all consumers?
                //var queueName = "OutageEventChangeNotificationQueue"; //channel.QueueDeclare().QueueName;
                //channel.QueueBind(queue: queue, exchange: exchange, routingKey: string.Empty);

                //Console.WriteLine(" [*] Waiting for outageEvents.");

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    Console.WriteLine($"[*] Received outageEvents bytes : {ea.Body.Length}.");

                    // Read out the headers and use the key value pairs for business or processing logic
                    if (ea.BasicProperties.IsHeadersPresent())
                    {
                        var headers = ea.BasicProperties.Headers;
                        foreach (var header in headers)
                        {
                            // strings are encoded at byte[] so we have to parse these out
                            // different than the other object values, more tests are needed
                            if (header.Value.GetType().Name == "Byte[]")
                            {
                                var headerBytes = (byte[])header.Value;
                                var value = Encoding.UTF8.GetString(headerBytes);
                                Console.WriteLine($"key : {header.Key} ; value : {value}");
                            }
                            else
                            {
                                var value = Convert.ToString(header.Value);
                                Console.WriteLine($"key : {header.Key}; value : {value}");
                            }
                        }
                    }

                    var body = ea.Body;
                    ProcessMessage(body);
                };
                channel.BasicConsume(queue: queue, autoAck: true, consumer: consumer);

                Console.WriteLine(" Press [enter] to exit.");
                Console.ReadLine();
            }
        }

        private void ProcessMessage(ReadOnlyMemory<byte> body)
        {
            var message = Encoding.UTF8.GetString(body.ToArray());
            Console.WriteLine($" Msg: {message}");

            var splits=message.Split(' ');
            //foreach (var item in splits)
            //{
            //    Console.WriteLine(item);
            //}

            OnReceived(new ReceivedEventArgs(new Detection() {
                ClassId = int.Parse(splits[0]),
                CenterX = float.Parse(splits[1]),
                CenterY = float.Parse(splits[2]),
                Width = float.Parse(splits[3]),
                Height = float.Parse(splits[4]),
                Confidence = float.Parse(splits[5]),
            }));

        }
    }
}
