using System.Text.Json;
using OrderServer.Models;

namespace OrderServer.Services
{
    public class OrderService
    {
        private readonly string _filePath = "Data/orders.csv";
        private readonly HttpClient _httpClient;
        private readonly string _catalogUrl;

        public OrderService(IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _httpClient = httpClientFactory.CreateClient();
            _catalogUrl = "http://catalog-service:8080";
        }

        public async Task<string> PurchaseAsync(int itemNumber)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_catalogUrl}/catalog/info/{itemNumber}");
                if (!response.IsSuccessStatusCode)
                    return "Catalog lookup failed.";

                var json = await response.Content.ReadAsStringAsync();
                var item = JsonSerializer.Deserialize<CatalogItem>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (item == null || item.Quantity <= 0)
                    return "Item out of stock.";

                var patch = new StringContent("");
                var patchResp = await _httpClient.PostAsync($"{_catalogUrl}/catalog/update/decrement/{itemNumber}", patch);
                if (!patchResp.IsSuccessStatusCode)
                    return "Failed to decrement stock.";

                var order = new Order
                {
                    Id = GenerateOrderId(),
                    ItemNumber = item.Id,
                    Title = item.Title,
                    OrderDate = DateTime.UtcNow
                };

                File.AppendAllText(_filePath, $"Student : {order.Id}, Item# :{order.ItemNumber}, Title: {order.Title}, Date : {order.OrderDate}\n");

                return $"Bought book {item.Title}";
            }
            catch (Exception ex)
            { 
                Console.WriteLine($"❌  Exception during stock decrement: {ex.Message}");
                return "Error during stock decrement.";
            }
        }
        private int GenerateOrderId()
        {
            if (!File.Exists(_filePath)) return 1;
            var last = File.ReadAllLines(_filePath).LastOrDefault();
            return last != null ? int.Parse(last.Split(',')[0]) + 1 : 1;
        }
    }
}
