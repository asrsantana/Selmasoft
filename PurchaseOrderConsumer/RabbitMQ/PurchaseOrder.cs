using System;

namespace PurchaseOrderConsumer
{
    [Serializable]
    public class PurchaseOrder
    {
        public decimal AmountToPay { get; set; }
        public string PoNumber { get; set; }
        public string CompanyName { get; set; }
        public int PaymentDayTerms { get; set; }
    }
}
