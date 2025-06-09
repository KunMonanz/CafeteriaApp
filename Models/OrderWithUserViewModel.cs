namespace CafeteriaApp.Models
{
    public class OrderWithUserViewModel
    {
        public int OrderId { get; set; }
        public string UserEmail { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public List<OrderItem> Items { get; set; } = new();
    }
}