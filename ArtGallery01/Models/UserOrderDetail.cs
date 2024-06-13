using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArtGallery01.Models
{
    [Table("UserOrderDetail")]
    public class UserOrderDetail
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Address { get; set; }
        [EmailAddress]
        public string email { get; set; }
        public int ShoppingCartId { get; set; }
        public decimal TotalAmount { get; set; }
        public ShoppingCart? ShoppingCart { get; set; }
        public List<OrderDetail>? OrderDetails { get; set; }

    }
}
