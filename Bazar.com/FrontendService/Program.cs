using System.Runtime.CompilerServices;
using System.Text.Json;
using UserAction.Utilites;

var infoCache = new Dictionary<int, (Book Data, DateTime CachedAt)>();
var searchCache = new Dictionary<string, (List<BookSearchResult> Data, DateTime CachedAt)>();
TimeSpan cacheTtl = TimeSpan.FromDays(1);

var httpClient = new HttpClient();
const string CATALOGNAME = "/catalog";
const string ORDERNAME = "/order";
var catalogUrls = (Environment.GetEnvironmentVariable("CATALOG_SERVICE_URL") ?? "")
    .Split(',', StringSplitOptions.RemoveEmptyEntries);
var orderUrls = (Environment.GetEnvironmentVariable("ORDER_SERVICE_URL") ?? "")
    .Split(',', StringSplitOptions.RemoveEmptyEntries);

int catalogIndex = 0, orderIndex = 0;
async Task<(string? Url, int NewIndex)> NextHealthyUrl(string[] urls, int startIdx, string serverType)
{
    string healthPath = serverType + "/health";
    if (urls.Length == 0) return (null, startIdx);
    int idx = startIdx;

    for (int i = 0; i < urls.Length; i++)
    {
        var url = urls[idx];
        idx = (idx + 1) % urls.Length;

        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(300));
            var resp = await httpClient.GetAsync($"{url}{healthPath}", cts.Token);
            if (resp.IsSuccessStatusCode)
                return (url, idx);
        }
        catch
        {
            // Ignore failure and try next
        }
    }

    return (null, startIdx);
}

Console.WriteLine("📚\tWelcome to the Online Bookstore! (with RR + cache)\n");

while (true)
{
    Console.WriteLine("1. View All Catalog Items");
    Console.WriteLine("2. View Catalog Item");
    Console.WriteLine("3. Purchase Book");
    Console.WriteLine("4. Search Book Topic");
    Console.WriteLine("5. Exit");
    Console.Write("Select an option: ");
    var input = Console.ReadLine();
    if (input == "1")
    {
        var (catalogUrl, newCatalogIndex) = await NextHealthyUrl(catalogUrls, catalogIndex,CATALOGNAME);
        if (catalogUrl == null)
        {
            Console.WriteLine("❌ All catalog services are down.");
            continue;
        }
        catalogIndex = newCatalogIndex;
        var resp = await httpClient.GetAsync($"{catalogUrl}/catalog/info");
        var books = JsonSerializer.Deserialize<List<Book>>(await resp.Content.ReadAsStringAsync(),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        Console.WriteLine("\n📚\t\tView All Books:");
        Console.WriteLine("{0,-40} {1,10} {2,10}", "Title", "Price", "Quantity");
        Console.WriteLine(new string('-', 70));
        foreach (var b in books)
            Console.WriteLine("{0,-40} {1,10:C} {2,10}", Truncate(b.Title, 40), b.Price, b.Quantity);
        Console.WriteLine("\n");
    }
    else if (input == "2")
    {
        Console.Write("\nEnter item ID: ");
        if (!int.TryParse(Console.ReadLine(), out var id)) continue;

        using (new RequestTimer(infoCache.ContainsKey(id) ? "Cache HIT (Info)" : "Cache MISS (Info)"))
        {
            if (infoCache.TryGetValue(id, out var entry) &&
                DateTime.UtcNow - entry.CachedAt < cacheTtl)
            {
                Console.WriteLine("🔄\tUsing cached data:");
                PrintBook(entry.Data);
                continue;
            }

            var (catalogUrl, newCatalogIndex) = await NextHealthyUrl(catalogUrls, catalogIndex, CATALOGNAME);
            if (catalogUrl == null)
            {
                Console.WriteLine("❌ All catalog services are down.");
                continue;
            }
            catalogIndex = newCatalogIndex;

            var resp = await httpClient.GetAsync($"{catalogUrl}/catalog/info/{id}");
            if (!resp.IsSuccessStatusCode) { Console.WriteLine("❌ Not found."); continue; }

            var book = JsonSerializer.Deserialize<Book>(await resp.Content.ReadAsStringAsync(),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

            infoCache[id] = (book, DateTime.UtcNow);
            Console.WriteLine("🔄\tCached data miss:");
            PrintBook(book);
        }
    }
    else if (input == "3")
    {
        Console.Write("Enter Book ID to purchase: ");
        if (!int.TryParse(Console.ReadLine(), out var id)) continue;

        var (orderUrl, newOrderIndex) = await NextHealthyUrl(orderUrls, orderIndex, ORDERNAME);
        if (orderUrl == null)
        {
            Console.WriteLine("❌ All order services are down.");
            continue;
        }
        orderIndex = newOrderIndex;

        var resp = await httpClient.PostAsync($"{orderUrl}/order/purchase/{id}", null);

        if (resp.IsSuccessStatusCode)
        {
            Console.WriteLine("✅ Order placed!");

            infoCache.Remove(id);
            searchCache.Clear();

            for (int i = 1; i < catalogUrls.Length; i++)
            {
                var url = catalogUrls[i];
                try
                {
                    await httpClient.PostAsync($"{url}/catalog/sync/decrement/{id}", null);
                    Console.WriteLine($"🔄 Synced decrement to: {url}");
                }
                catch
                {
                    Console.WriteLine($"⚠️ Failed to sync with {url}");
                }
            }

        }
        else
        {
            var err = await resp.Content.ReadAsStringAsync();
            Console.WriteLine($"❌  {err}");
        }
    }
    else if (input == "4")
    {
        Console.Write("Enter topic: ");
        var topic = Console.ReadLine() ?? "";

        using (new RequestTimer(searchCache.ContainsKey(topic) ? "Cache HIT (Search)" : "Cache MISS (Search)"))
        {
            if (searchCache.TryGetValue(topic, out var sEntry) &&
                DateTime.UtcNow - sEntry.CachedAt < cacheTtl)
            {
                PrintSearch(sEntry.Data);
                continue;
            }

            var (catalogUrl, newCatalogIndex) = await NextHealthyUrl(catalogUrls, catalogIndex, CATALOGNAME);
            if (catalogUrl == null)
            {
                Console.WriteLine("❌ All catalog services are down.");
                return;
            }
            catalogIndex = newCatalogIndex;

            var resp = await httpClient.GetAsync($"{catalogUrl}/catalog/search/{topic}");
            var results = JsonSerializer.Deserialize<List<BookSearchResult>>(await resp.Content.ReadAsStringAsync(),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

            if (results.Count == 0)
                Console.WriteLine($"😕 No books under \"{topic}\".");
            else
            {
                searchCache[topic] = (results, DateTime.UtcNow);
                PrintSearch(results);
            }
        }
    }
    else if (input == "5") break;
    else Console.WriteLine("Invalid. Try 1–5.");
}

void PrintBook(Book b) =>
    Console.WriteLine($"\n[ID:{b.Id}] {b.Title} — ${b.Price} — Qty: {b.Quantity}\n");

void PrintSearch(IEnumerable<BookSearchResult> list)
{
    Console.WriteLine("\n🔍 Search results:");
    foreach (var r in list)
        Console.WriteLine($"• [{r.Id}] {r.Title}");
    Console.WriteLine();
}

string Truncate(string v, int len) =>
    v.Length <= len ? v : v[..(len - 3)] + "...";

record Book(int Id, string Title, decimal Price, int Quantity);
record BookSearchResult(int Id, string Title);
