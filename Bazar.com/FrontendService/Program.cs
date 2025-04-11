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
    Console.WriteLine("4. Exit");
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
    if (input == "2")
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
        Console.WriteLine($"Book: {book.Title} - Price: ${book.Price} - Quantity: {book.Quantity}");
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
        Console.WriteLine($"👉 Sending POST to: {fullUri}");
        var orderResp = await httpClient.PostAsync(fullUri, null);

        if (orderResp.IsSuccessStatusCode)
        {
            Console.WriteLine("✅  Order placed successfully!");
        }
        else
        {
            Console.WriteLine("❌  Failed to place order.");
            var error = await orderResp.Content.ReadAsStringAsync();
            Console.WriteLine($"Error body: {error}");
        }
    }
    else if (input == "4")
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
record Book(string Title, decimal Price, int Quantity);

