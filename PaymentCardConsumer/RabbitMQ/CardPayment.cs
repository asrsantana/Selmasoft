using System;

namespace PaymentCardConsumer
{
    [Serializable]
    public class CardPayment
    {
        public decimal AmountToPay { get; set; }
        public string CardNumber { get; set; }
        public string Name { get; set; }
    }
}
