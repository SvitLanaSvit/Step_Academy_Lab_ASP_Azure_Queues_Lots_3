namespace Lab_ASP_Azure_Queues_3.Models.ViewModels
{
    public class GetLotViewModel
    {
        public IEnumerable<AzureLot> AzureLots { get; set; } = default!;
        public string? Currency { get; set; } = default!;
    }
}
