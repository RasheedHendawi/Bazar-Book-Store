using CatalogServer.Models;
using CatalogServer.Services;
using Microsoft.AspNetCore.Mvc;

namespace CatalogServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CatalogController : ControllerBase
    {
        private readonly BookRepository _bookRepository;
        public CatalogController(BookRepository bookRepository)
        {
            _bookRepository = bookRepository;
        }

        [HttpGet("search/{topic}")]
        public IActionResult Search(string topic)
        {
            var books = _bookRepository.GetByTopic(topic).Select(b => new { b.Id, b.Title });
            return Ok(books);
        }

        [HttpGet("info/{id}")]
        public IActionResult Info(int id)
        {
            var book = _bookRepository.GetById(id);
            if (book == null)
            {
                return NotFound();
            }
            return Ok(new { book.Title, book.Quantity, book.Price });
        }
        [HttpGet("info")]
        public IActionResult GetAllBooks()
        {
            var books = _bookRepository.GetAll();
            if (books == null)
            {
                return NotFound();
            }
            return Ok(books);
        }
        [HttpPost("update/decrement/{id}")]
        public IActionResult Update(int id)
        {
            var book = _bookRepository.GetById(id);
            if (book == null)
                return NotFound();
            Console.WriteLine($"quan {book.Quantity}");
            if (book.Quantity <= 0)
                return BadRequest("Out of stock");

            book.Quantity--;
            _bookRepository.Update(book);

            return Ok(book);
        }
    }
}
