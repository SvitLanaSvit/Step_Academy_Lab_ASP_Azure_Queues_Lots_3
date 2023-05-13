namespace Lab_ASP_Azure_Queues_3.Models
{
    public class AzureLot
    {
        public string MessageId { get; set; } = default!;
        public CurrencyLot Lot { get; set; } = default!;
    }
}
