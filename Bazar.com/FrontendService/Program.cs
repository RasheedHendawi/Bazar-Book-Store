using System.Text;
using System.Text.Json;

Console.WriteLine("📚 Welcome to the Online Bookstore!");

var catalogUrl = "http://catalog-service:8080";
var orderUrl = "http://order-service:8080";

var httpClient = new HttpClient();

while (true)
{
    Console.WriteLine("\n1. View All Catalog Items");
    Console.WriteLine("2. View Catalog Item");
    Console.WriteLine("3. Purchase Book");
    Console.WriteLine("4. Search Book Topic");
    Console.WriteLine("5. Exit");
    Console.Write("Select an option: ");
    var input = Console.ReadLine();
    if (input == "1")
    {
        var resp = await httpClient.GetAsync($"{catalogUrl}/catalog/info");
        var json = await resp.Content.ReadAsStringAsync();
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var books = JsonSerializer.Deserialize<List<Book>>(json, options);

        Console.WriteLine("\n📚 All Books in the Catalog:");
        Console.WriteLine("{0,-40} {1,10} {2,10}", "Title", "Price", "Quantity");
        Console.WriteLine(new string('-', 70));

        foreach (var b in books!)
        {
            Console.WriteLine("{0,-40} {1,10:C} {2,10}",Truncate(b.Title, 40), b.Price, b.Quantity);
        }
    }
    else if (input == "2")
    {
        string? userItem;
        Console.Write("\nEnter the ID of item: ");
        userItem = Console.ReadLine();
        if (string.IsNullOrEmpty(userItem))
        {
            Console.WriteLine("Invalid input. Please enter a valid item number.");
            continue;
        }
        if (!int.TryParse(userItem, out var itemNumber))
        {
            Console.WriteLine("Invalid input. Please enter a valid item number. (integer)");
            continue;
        }
        var resp = await httpClient.GetAsync($"{catalogUrl}/catalog/info/{itemNumber}");
        var json = await resp.Content.ReadAsStringAsync();
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        var book = JsonSerializer.Deserialize<Book>(json, options);
        Console.WriteLine($"\nThe Book Requested with ID {itemNumber}:");
        Console.WriteLine($"Book_ID : {book.Id} Book: {book.Title} - Price: ${book.Price} - Quantity: {book.Quantity}");
    }
    else if (input == "3")
    {
        Console.Write("Enter Book ID to purchase: ");
        if (!int.TryParse(Console.ReadLine(), out var bookId)) continue;

        //Console.Write("Enter Quantity: ");
        //if (!int.TryParse(Console.ReadLine(), out var qty)) continue;

        var order = new
        {
            ItemNumber = bookId,
            //Quantity = qty
        };

        var fullUri = $"{orderUrl}/order/purchase/{bookId}";
        var orderResp = await httpClient.PostAsync(fullUri, null);

        if (orderResp.IsSuccessStatusCode)
        {
            Console.WriteLine($"📦 {orderResp.StatusCode}");
            Console.WriteLine("✅  Order placed successfully!");
        }
        else
        {
            Console.WriteLine("❌  Failed to place order.");
        }
    }
    else if (input == "4")
    {
        Console.Write("🔍 Enter topic to search: ");
        var topic = Console.ReadLine();

        var response = await httpClient.GetAsync($"{catalogUrl}/catalog/search/{topic}");
        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine("❌ Failed to search catalog.");
            return;
        }

        var json = await response.Content.ReadAsStringAsync();
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var results = JsonSerializer.Deserialize<List<BookSearchResult>>(json, options);

        if (results == null || results.Count == 0)
        {
            Console.WriteLine($"😕 No books found under the topic \"{topic}\".");
        }
        else
        {
            Console.WriteLine($"\n📚 Books found for topic \"{topic}\":");
            foreach (var b in results)
            {
                Console.WriteLine($"🔸 ID: {b.Id}, Title: {b.Title}");
            }
        }
    }
    else if (input == "5")
    {
        break;
    }
    else
    {
        Console.WriteLine("Invalid option.");
    }
}
string Truncate(string value, int maxLength)
{
    return value.Length <= maxLength ? value : value.Substring(0, maxLength - 3) + "...";
}
record Book(int Id,string Title, decimal Price, int Quantity);
record BookSearchResult(int Id, string Title);
