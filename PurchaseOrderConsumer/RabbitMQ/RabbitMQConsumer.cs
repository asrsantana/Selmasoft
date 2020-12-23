using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace PurchaseOrderConsumer
{
    public class RabbitMQConsumer
    {
        private static ConnectionFactory _factory;
        private static IConnection _connection;

        private const string ExchangeName = "Topic_Exchange";
        private const string PurchaseOrderQueueName = "PurchaseOrderTopic_Queue";


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
                Console.WriteLine("Listening for Topic <payment.purchaseorder>");
                Console.WriteLine("-----------------------------------------");
                Console.WriteLine();

                channel.ExchangeDeclare(ExchangeName, ExchangeType.Topic);
                channel.QueueDeclare(PurchaseOrderQueueName, true, false, false, null);
                channel.QueueBind(PurchaseOrderQueueName, ExchangeName, "payment.purchaseorder");

                channel.BasicQos(0, 1, false);

                var consumer = new EventingBasicConsumer(channel);

                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var message = (PurchaseOrder)body.DeSerialize(typeof(PurchaseOrder));
                    var routingKey = ea.RoutingKey;
                    channel.BasicAck(ea.DeliveryTag, true);
                    Console.WriteLine($"... Purchase Order = Routing Key {routingKey} " +
                                      $": {message.CompanyName} : {message.PoNumber} : {message.AmountToPay} ");
 
                };

                channel.BasicConsume(queue: PurchaseOrderQueueName,
                                     autoAck: false,
                                     consumer: consumer);

                Console.WriteLine(" Press [enter] to exit.");
                Console.ReadLine();

            }

        }
    }
}
