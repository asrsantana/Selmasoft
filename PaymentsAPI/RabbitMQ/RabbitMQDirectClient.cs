using Common;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Concurrent;
using System.Text;

namespace PaymentsAPI.RabbitMQ
{
    public class RabbitMQDirectClient
    {
        private static ConnectionFactory factory;
        private static IConnection connection;
        private static IModel channel;
        private string replyQueueName;
        private EventingBasicConsumer consumer;
        private IBasicProperties props;
        private readonly BlockingCollection<string> respQueue = new BlockingCollection<string>();

        private const string ExchangeName = "Topic_Exchange";

        public RabbitMQDirectClient()
        {
            CreateConnection();
        }

        public void CreateConnection()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };

            connection = factory.CreateConnection();
            channel = connection.CreateModel();
            replyQueueName = channel.QueueDeclare().QueueName;
            consumer = new EventingBasicConsumer(channel);

            props = channel.CreateBasicProperties();


         
        }

        public void MakePayment(CardPayment payment)
        {
            channel.QueueDeclare(queue: "rpc_queue", durable: false,
             exclusive: false, autoDelete: false, arguments: null);

            channel.BasicQos(0, 1, false);

            var consumer = new EventingBasicConsumer(channel);

            channel.BasicConsume(queue: "rpc_queue",autoAck: false, consumer: consumer);
            Console.WriteLine(" [x] Awaiting RPC requests");

            consumer.Received += (model, ea) =>
            {
                string response = null;

                var body = ea.Body.ToArray();
                var props = ea.BasicProperties;
                var replyProps = channel.CreateBasicProperties();
                replyProps.CorrelationId = props.CorrelationId;

                try
                {
                    response = Encoding.UTF8.GetString(body);
                   
                    Console.WriteLine(" [.] fib({0})", response);
                    
                }
                catch (Exception e)
                {
                    Console.WriteLine(" [.] " + e.Message);
                    response = "";
                }
                finally
                {
                    var responseBytes = Encoding.UTF8.GetBytes(response);
                    channel.BasicPublish("", props.ReplyTo,replyProps,responseBytes);
                    channel.BasicAck(deliveryTag: ea.DeliveryTag,
                      multiple: false);
                }
            };

            Console.WriteLine(" Press [enter] to exit.");
 
        }

 
        public void Close()
        {
            connection.Close();
        }
    }
}
