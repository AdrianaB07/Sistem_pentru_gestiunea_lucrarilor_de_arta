namespace ArtGallery01.Repositories
{
    public interface IWishlistRepository
    {
        Task<int> AddItem(int productId, int qty);
        Task<int> RemoveItem(int productId);
        Task<Wishlist> GetUserWishlist();
        Task<int> GetWishlistItemCount(string userId = "");
        Task<Wishlist> GetWishlist(string userId);
        Task<bool> DoCheckout();
    }
}
