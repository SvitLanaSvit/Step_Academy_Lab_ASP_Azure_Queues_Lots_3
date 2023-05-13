namespace Lab_ASP_Azure_Queues_3.Models.ViewModels
{
    public class BuyLotViewModel
    {
        public string MessageId { get; set; } = default!;
        public CurrencyLot Lot { get; set; } = default!;
        public string PopReceipt { get; set; } = default!;
    }
}
