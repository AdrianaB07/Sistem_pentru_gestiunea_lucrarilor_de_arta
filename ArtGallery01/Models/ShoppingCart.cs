using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArtGallery01.Models
{
    [Table("ShoppingCart")]
    public class ShoppingCart
    {
        public int Id { get; set; }
        [Required]
        public string UserId { get; set; }
        public bool IsDeleted { get; set; } = false;
        public ICollection<CartDetail>? CartDetails { get; set; }
        public ICollection<UserOrderDetail>? UserOrderDetails { get; set; }
    }

}