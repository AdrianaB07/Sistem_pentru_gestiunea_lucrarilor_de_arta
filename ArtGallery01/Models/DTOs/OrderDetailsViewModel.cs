namespace ArtGallery01.Models.DTOs
{
    public class OrderDetailsViewModel
    {
        public Order Order { get; set; }
        public List<OrderDetail> OrderDetails { get; set; }
        public UserOrderDetail UserOrderDetail { get; set; }
    }
}
