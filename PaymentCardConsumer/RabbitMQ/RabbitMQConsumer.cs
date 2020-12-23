using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace PaymentCardConsumer.RabbitMQ
{
    public class RabbitMQConsumer
    {
        private static ConnectionFactory _factory;
        private static IConnection _connection;

        private const string ExchangeName = "Topic_Exchange";
        private const string CardPaymentQueueName = "CardPaymentTopic_Queue";


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
                Console.WriteLine("Listening for Topic <payment.cardpayment>");
                Console.WriteLine("-----------------------------------------");
                Console.WriteLine();

                channel.ExchangeDeclare(ExchangeName, ExchangeType.Topic);
                channel.QueueDeclare(CardPaymentQueueName, true, false, false, null);
                channel.QueueBind(CardPaymentQueueName, ExchangeName, "payment.cardpayment");

                channel.BasicQos(0, 1, false);

                var consumer = new EventingBasicConsumer(channel);

                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var message = (CardPayment)body.DeSerialize(typeof(CardPayment));
                    var routingKey = ea.RoutingKey;
                    channel.BasicAck(ea.DeliveryTag, true);
                    Console.WriteLine($"... Payment = Routing Key {routingKey} " +
                                      $": {message.CardNumber} : {message.Name} : {message.AmountToPay} ");
 
                   
                };

                channel.BasicConsume(queue: CardPaymentQueueName,
                                     autoAck: false,
                                     consumer: consumer);


                Console.WriteLine(" Press [enter] to exit.");
                Console.ReadLine();
            }
         
        }
    }
}
