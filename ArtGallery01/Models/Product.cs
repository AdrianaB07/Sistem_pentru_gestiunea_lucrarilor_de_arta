using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArtGallery01.Models
{
    [Table("Product")]
    public class Product
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(40)]
        public string? ProductName { get; set; }
        [Required]
        public double Price { get; set; }
        public string? Image { get; set; }
        public string? Description { get; set; }
        [Required]
        public int CategoryId { get; set; }
        public Category? Category { get; set; }
        public List<OrderDetail>? OrderDetail { get; set; }
        public List<WishlistDetail>? WishlistDetail { get; set; }
        public List<CartDetail>? CartDetail { get; set; }
        [NotMapped]
        public string? CategoryName { get; set; }
        public string? NewCommentContent { get; set; }
        public List<Comment>? Comments { get; set; }
        public bool IsFeatured { get; set; }
        public string? OwnerId { get; set; }
        [NotMapped]
        public string? OwnerEmail { get; set; }
        public List<Tag> Tags { get; set; } = new List<Tag>();
        public bool IsSold { get; set; }

    }
}
