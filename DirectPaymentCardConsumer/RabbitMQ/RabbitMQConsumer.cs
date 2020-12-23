using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;

namespace DirectPaymentCardConsumer
{
    public class RabbitMQConsumer
    {
        private static ConnectionFactory _factory;
        private static IConnection _connection;
        private static IModel _channel;
        private EventingBasicConsumer _consumer;

        internal void CreateConnection()
        {
            _factory = new ConnectionFactory
            {
                HostName = "localhost",
                UserName = "guest",
                Password = "guest"
            };
            _connection = _factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.QueueDeclare("rpc_queue", true, false, false, null);
            _channel.BasicQos(0, 1, false);
            _consumer = new EventingBasicConsumer(_channel);
        }

        internal void ProcessMessages()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "rpc_queue", true, false, false, null);

                channel.BasicQos(0, 1, false);

                var consumer = new EventingBasicConsumer(channel);

                channel.BasicConsume("rpc_queue", false, consumer);

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
                        response = MakePayment(ea);
                        Console.WriteLine($" Correlation ID = {props.CorrelationId} ");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(" [.] " + e.Message);
                        response = "";
                    }
                    finally
                    {
                        var responseBytes = Encoding.UTF8.GetBytes(response);

                        channel.BasicPublish(exchange: "",
                                             routingKey: props.ReplyTo,
                                             basicProperties: replyProps,
                                             body: responseBytes);

                        channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                    }
                };

                Console.WriteLine(" Press [enter] to exit.");
                Console.ReadLine();
            }
        }

        private string MakePayment(BasicDeliverEventArgs ea)
        {
            var body = ea.Body.ToArray();
            var payment = (CardPayment)body.DeSerialize(typeof(CardPayment));
            Console.WriteLine($"... Payment >> {payment.CardNumber} : {payment.Name} : {payment.AmountToPay} ");
            return "";
        }
    }
}
