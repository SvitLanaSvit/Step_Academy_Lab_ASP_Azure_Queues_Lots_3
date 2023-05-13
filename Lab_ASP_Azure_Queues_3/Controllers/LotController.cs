using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Lab_ASP_Azure_Queues_3.Models;
using Lab_ASP_Azure_Queues_3.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Lab_ASP_Azure_Queues_3.Controllers
{
    public class LotController : Controller
    {
        private readonly string queueName = "lots";
        private readonly QueueServiceClient serviceClient;
        private readonly ILogger<LotController> logger;

        public LotController(QueueServiceClient serviceClient, ILoggerFactory loggerFactory) {
            this.serviceClient = serviceClient;
            logger = loggerFactory.CreateLogger<LotController>();
        }

        public async Task<IActionResult> Index()
        {
            QueueClient queueClient = await CreateQueueClient(queueName);
            var azureResponse = await queueClient.PeekMessagesAsync(maxMessages: 10);
            PeekedMessage[] lots = azureResponse.Value;
            
            return View(lots);
        }

        public async Task<QueueClient> CreateQueueClient(string queueName)
        {
            QueueClient queueClient = serviceClient.GetQueueClient(queueName);
            await queueClient.CreateIfNotExistsAsync();
            return queueClient;
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CurrencyLot lot)
        {
            if (!ModelState.IsValid)
            {
                return View(lot);
            }
            QueueClient queueClient = serviceClient.GetQueueClient(queueName);
            string messageJson = JsonSerializer.Serialize(lot);
            SendReceipt receipt = await queueClient.SendMessageAsync(messageJson, 
                timeToLive: TimeSpan.FromDays(1));
            logger.LogInformation($"MessageId: {receipt.MessageId}");
            logger.LogInformation($"Expiration Time: {receipt.ExpirationTime}");
            logger.LogInformation($"Pop Receipt: {receipt.PopReceipt}");
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Get(string? currency)
        {
            try
            {
                QueueClient queueClient = serviceClient.GetQueueClient(queueName);
                
                int batchSize = 10;
                int visibilityTimeoutInSeconds = 30;

                if (currency == null)
                {
                    List<AzureLot> azureLots = new List<AzureLot>();

                    foreach (var message in (await queueClient
                    .PeekMessagesAsync(maxMessages: batchSize)).Value)
                    {
                        CurrencyLot? lot = JsonSerializer.Deserialize<CurrencyLot>(message.Body.ToString());
                        AzureLot azureLot = new AzureLot()
                        {
                            MessageId = message.MessageId,
                            Lot = lot!
                        };
                        
                        azureLots.Add( azureLot );
                    }

                    GetLotViewModel viewModel = new GetLotViewModel()
                    {
                        AzureLots = azureLots
                    };
                    return View(viewModel);
                }
                else
                {
                    List<AzureLot> azureLots = new List<AzureLot>();

                    foreach (QueueMessage message in (await queueClient
                    .ReceiveMessagesAsync(maxMessages: batchSize, TimeSpan.FromSeconds(visibilityTimeoutInSeconds))).Value)
                    {

                        CurrencyLot? lot = JsonSerializer.Deserialize<CurrencyLot>(message.Body.ToString());
                        if(lot!.Currency == currency)
                        {
                            AzureLot azureLot = new AzureLot()
                            {
                                MessageId = message.MessageId,
                                Lot = lot!
                            };
                            azureLots.Add( azureLot );
                        }
                    }

                    //ViewBag.Currency = currency;
                    GetLotViewModel viewModel = new GetLotViewModel()
                    {
                        Currency = currency,
                        AzureLots = azureLots
                    };
                    return View(viewModel);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to get lots from the queue");
                return StatusCode(500, "Failed to get lots from the queue");
            }
        }

        public IActionResult Buy(AzureLot azureLot) 
        {
            return View(azureLot);
        }

        [HttpPost, ActionName("Buy")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BuyConfirmed(AzureLot azureLot)
        {
            try
            {
                if (!ModelState.IsValid)
                    return View("Buy", azureLot);

                QueueClient queueClient = serviceClient.GetQueueClient(queueName);
                var response = await queueClient.ReceiveMessagesAsync();

                BuyLotViewModel buyLot = new();

                foreach (QueueMessage message in response.Value)
                {
                    if (message.MessageId == buyLot.MessageId)
                    {
                        buyLot.MessageId = message.MessageId;
                        buyLot.Lot = message.Body.ToObjectFromJson<CurrencyLot>();
                        buyLot.PopReceipt = message.PopReceipt;
                    }
                }
                await queueClient.DeleteMessageAsync(buyLot.MessageId, buyLot.PopReceipt);

                return View("Index");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to delete the lot from the queue");
                return StatusCode(500, "Failed to delete the lot from the queue");
            }
        }
    }
}
