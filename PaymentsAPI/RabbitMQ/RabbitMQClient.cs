using Common;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PaymentsAPI.RabbitMQ
{
    public class RabbitMQClient
    {
        private static ConnectionFactory _factory;
        private static IConnection _connection;
        private static IModel _model;

        private const string ExchangeName = "Topic_Exchange";
        private const string CardPaymentQueueName = "CardPaymentTopic_Queue";
        private const string PurchaseOrderQueueName = "PurchaseOrderTopic_Queue";
        private const string AllQueueName = "AllTopic_Queue";

        public RabbitMQClient()
        {
            CreateConnection();
        }

        private void CreateConnection()
        {
            _factory = new ConnectionFactory
            {
                HostName = "localhost",
                UserName = "guest",
                Password = "guest"
            };
            _connection = _factory.CreateConnection();
            _model = _connection.CreateModel();
            _model.ExchangeDeclare(ExchangeName, ExchangeType.Topic);

            _model.QueueDeclare(CardPaymentQueueName, true, false, false, null);
            _model.QueueDeclare(PurchaseOrderQueueName, true, false, false, null);
            _model.QueueDeclare(AllQueueName, true, false, false, null);

            _model.QueueBind(CardPaymentQueueName, ExchangeName, "payment.card");
            _model.QueueBind(PurchaseOrderQueueName, ExchangeName, "payment.purchaseorder");
            _model.QueueBind(AllQueueName, ExchangeName, "payment.*");
        }

        public void SendPayment(CardPayment payment)
        {
            SendMessage(payment.Serialize(), "payment.card");
            Console.WriteLine($" Payment Sent {payment.CardNumber} : {payment.AmountToPay}");
        }

        public void SendPurchaseOrder(PurchaseOrder purchaseOrder)
        {
            SendMessage(purchaseOrder.Serialize(), "payment.purchaseorder");
            Console.WriteLine(" Purchase Order Sent {0}, {1}, {2}, {3}",
                purchaseOrder.CompanyName,
                purchaseOrder.AmountToPay,
                purchaseOrder.PaymentDayTerms,
                purchaseOrder.PoNumber);
        }

        public void SendMessage(byte[] message, string routingKey)
        {
            _model.BasicPublish(ExchangeName, routingKey, null, message);
        }

        public void Close()
        {
            _connection.Close();
        }
    }
}
