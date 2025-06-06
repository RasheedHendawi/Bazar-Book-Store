using CatalogServer.Models;
using System.Text.Json;
using System.Xml.Linq;

namespace CatalogServer.Services
{
    public class BookRepository
    {
        private readonly string _filePath = "Data/catalog.json";
        private List<Book> _books = new();
        public BookRepository()
        {
            if (File.Exists(_filePath))
            {
                var json = File.ReadAllText(_filePath);
                _books = JsonSerializer.Deserialize<List<Book>>(json) ?? new();
            }
            else
            {
                Seed();
            }
        }
        private void Seed()
        {
            _books = new List<Book>
        {
            new Book { Id = 1, Title = "How to get a good grade in DOS in 40 minutes a day", Topic = "distributed systems", Quantity = 5, Price = 40 },
            new Book { Id = 2, Title = "RPCs for Noobs", Topic = "distributed systems", Quantity = 3, Price = 50 },
            new Book { Id = 3, Title = "Xen and the Art of Surviving Undergraduate School", Topic = "undergraduate school", Quantity = 7, Price = 30 },
            new Book { Id = 4, Title = "Cooking for the Impatient Undergrad", Topic = "undergraduate school", Quantity = 4, Price = 25 },
            new Book { Id = 5, Title = "How to finish Project 3 on time", Topic = "undergraduate school", Quantity = 6, Price = 35 },
            new Book { Id = 6, Title = "Why theory classes are so hard.", Topic = "undergraduate school", Quantity = 12, Price = 55 },
            new Book { Id = 7, Title = "Spring in the Pioneer Valley", Topic = "undergraduate school", Quantity = 10, Price = 20 },
        };
            SaveChanges();
        }
        private void SaveChanges()
        {
            var json = JsonSerializer.Serialize(_books, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_filePath, json);
        }
        public IEnumerable<Book> GetByTopic(string topic)
        {
            return _books.Where(b => b.Topic.Equals(topic, StringComparison.OrdinalIgnoreCase));
        }
        public Book? GetById(int id)
        {
            return _books.FirstOrDefault(b => b.Id == id);
        }
        public IEnumerable<Book> GetAll()
        {
            return _books;
        }
        public void Update(Book book)
        {
            var index = _books.FindIndex(b => b.Id == book.Id);
            if (index != -1)
            {
                _books[index] = book;
                SaveChanges();
            }
        }
    }
}
