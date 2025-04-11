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
                    throw new Exception("❌Item not found or out of stock.");

                var patch = new StringContent("");
                var patchResp = await _httpClient.PostAsync($"{_catalogUrl}/catalog/update/decrement/{itemNumber}", patch);
                if (!patchResp.IsSuccessStatusCode)
                    throw new Exception("❌Failed to decrement stock.");

                var order = new Order
                {
                    Id = GenerateOrderId(),
                    ItemNumber = item.Id,
                    Title = item.Title,
                    OrderDate = DateTime.UtcNow
                };

                File.AppendAllText(_filePath, $"{order.Id},{order.ItemNumber},\"{order.Title}\",{order.OrderDate:o}\n");


                return $"Bought book {item.Title}";
            }
            catch (Exception ex)
            { 
                throw new Exception(ex.Message);
            }
        }
        private int GenerateOrderId()
        {
            if (!File.Exists(_filePath)) return 1;

            var lines = File.ReadAllLines(_filePath);
            var last = lines.LastOrDefault(line => !string.IsNullOrWhiteSpace(line));

            if (last == null)
                return 1;

            var parts = last.Split(',');

            if (parts.Length < 1 || !int.TryParse(parts[0], out var lastId))
                return 1;

            return lastId + 1;
        }
    }
}
