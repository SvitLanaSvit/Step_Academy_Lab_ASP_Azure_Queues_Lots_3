namespace Lab_ASP_Azure_Queues_3.Models
{
    public class CurrencyLot
    {
        public string Currency { get; set; } = default!;
        public decimal Amount { get; set; }
        public string SellerName { get; set; } = default!;
    }
}
