namespace OrderServer.Models
{
    public class Order
    {
        public int Id { get; set; }
        public int ItemNumber { get; set; }
        public string Title { get; set; }
        public DateTime OrderDate { get; set; }
    }
}
