namespace ArtGallery01.Models
{
    public class Tag
    {
        public int Id { get; set; }
        public string? Content { get; set; }
        public int? ProductId { get; set; }
        public Product? Product { get; set; }
    }
}
