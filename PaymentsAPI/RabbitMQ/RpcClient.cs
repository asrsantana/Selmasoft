using System;
using System.Collections.Concurrent;
using System.Text;
using Common;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

public class RpcClient
{
    private IConnection connection;
    private IModel channel;
    private string replyQueueName;
    private EventingBasicConsumer consumer;
    private BlockingCollection<string> respQueue = new BlockingCollection<string>();
    private IBasicProperties props;

    public void CreateConnection()
    {
        var factory = new ConnectionFactory() { HostName = "localhost" };

        connection = factory.CreateConnection();

        channel = connection.CreateModel();

        consumer = new EventingBasicConsumer(channel);
    }

    public string MakePayment(CardPayment payment)
    {
        replyQueueName = channel.QueueDeclare("rpc_reply", true, false, false, null).QueueName;

        props = channel.CreateBasicProperties();

        var correlationId = Guid.NewGuid().ToString();
        props.CorrelationId = correlationId;
        props.ReplyTo = replyQueueName;

        consumer.Received += (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var response = Encoding.UTF8.GetString(body);
            if (ea.BasicProperties.CorrelationId == correlationId)
            {
                respQueue.Add(response);
            }
        };

        channel.BasicPublish(
            exchange: "",
            routingKey: "rpc_queue",
            basicProperties: props,
            body: payment.Serialize());

        channel.BasicConsume(
            consumer: consumer,
            queue: replyQueueName,
            autoAck: true);

        return respQueue.Take();
    }

    public void Close()
    {
        connection.Close();
    }
}
 