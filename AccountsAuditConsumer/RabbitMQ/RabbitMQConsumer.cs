using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace AccountsAuditConsumer
{
    public class RabbitMQConsumer
    {
        private static ConnectionFactory _factory;
        private static IConnection _connection;

        private const string ExchangeName = "Topic_Exchange";
        private const string AllQueueName = "AllTopic_Queue";


        internal void CreateConnection()
        {
            _factory = new ConnectionFactory
            {
                HostName = "localhost",
                UserName = "guest",
                Password = "guest"
            };
        }

        internal void Close()
        {
            _connection.Close();
        }

        internal void ProcessMessages()
        {
            using (_connection = _factory.CreateConnection())
            using (var channel = _connection.CreateModel())
            {
                Console.WriteLine("Listening for Topic <payment.*>");
                Console.WriteLine("-----------------------------------------");
                Console.WriteLine();

                channel.ExchangeDeclare(ExchangeName, ExchangeType.Topic);
                channel.QueueDeclare(AllQueueName, true, false, false, null);
                channel.QueueBind(AllQueueName, ExchangeName, "payment.*");

                channel.BasicQos(0, 10, false);

                var consumer = new EventingBasicConsumer(channel);

                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var routingKey = ea.RoutingKey;

                    if (routingKey == "payment.card")
                    {
                        var message = (CardPayment)body.DeSerialize(typeof(CardPayment));
                        Console.WriteLine($"... Payment Card = Routing Key {routingKey} " +
                                          $": {message.Name} : {message.CardNumber} : {message.AmountToPay} ");
                        channel.BasicAck(ea.DeliveryTag, true);              
                    }

                    if (routingKey == "payment.purchaseorder")
                    {
                        var message = (PurchaseOrder)body.DeSerialize(typeof(PurchaseOrder));
                        Console.WriteLine($"... Purchase Order = Routing Key {routingKey} " +
                                          $": {message.CompanyName} : {message.PoNumber} : {message.AmountToPay} ");
                        channel.BasicAck(ea.DeliveryTag, true);
                    }                          
                };

                channel.BasicConsume(queue: AllQueueName,
                                     autoAck: false,
                                     consumer: consumer);

                Console.WriteLine(" Press [enter] to exit.");
                Console.ReadLine();

            }

        }
    }
}
