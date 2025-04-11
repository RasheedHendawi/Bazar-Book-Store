
namespace CatalogServer.Models
{
    public class Book
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public required string Topic { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
