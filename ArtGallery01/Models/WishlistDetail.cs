using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArtGallery01.Models
{
    [Table("WishlistDetail")]
    public class WishlistDetail
    {
        public int Id { get; set; }
        [Required]
        public int WishlistId { get; set; }
        [Required]
        public int ProductId { get; set; }
        [Required]
        public int Quantity { get; set; }
        [Required]
        public double UnitPrice { get; set; }   
        public Product Product { get; set; }
        public Wishlist Wishlist { get; set; }
    }
}
