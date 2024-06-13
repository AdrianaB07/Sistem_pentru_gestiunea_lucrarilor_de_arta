using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArtGallery01.Models
{
    [Table("Comment")]
    public class Comment
    {
        public int Id { get; set; }
        public string? Content { get; set; }
        public DateTime DateAdded { get; set; }

        public string? UserId { get; set; }
        public IdentityUser? User { get; set; }

        public int? ProductId { get; set; }
        public Product? Product { get; set; }
    }
}
