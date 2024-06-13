using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace ArtGallery01.Repositories
{
    public class WishlistRepository : IWishlistRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public WishlistRepository(ApplicationDbContext db, IHttpContextAccessor httpContextAccessor,
            UserManager<IdentityUser> userManager)
        {
            _db = db;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<int> AddItem(int productId, int qty)
        {
            string userId = GetUserId();
            using var transaction = _db.Database.BeginTransaction();
            try
            {
                if (string.IsNullOrEmpty(userId))
                    throw new Exception("User is not logged-in");
                var wishlist = await GetWishlist(userId);
                if (wishlist is null)
                {
                    wishlist = new Wishlist
                    {
                        UserId = userId
                    };
                    _db.Wishlists.Add(wishlist);
                }
                _db.SaveChanges();
                // Wishlist detail section
                var wishlistItem = _db.WishlistDetails
                    .FirstOrDefault(a => a.WishlistId == wishlist.Id && a.ProductId == productId);
                if (wishlistItem is null)
                {
                    var product = _db.Products.Find(productId);
                    wishlistItem = new WishlistDetail
                    {
                        ProductId = productId,
                        WishlistId = wishlist.Id,
                        Quantity = 1, // This field is irrelevant now
                        UnitPrice = product.Price
                    };
                    _db.WishlistDetails.Add(wishlistItem);
                }
                _db.SaveChanges();
                transaction.Commit();
            }
            catch (Exception ex)
            {
                // Log the exception
            }
            var wishlistItemCount = await GetWishlistItemCount(userId);
            return wishlistItemCount;
        }


        public async Task<int> RemoveItem(int productId)
        {
            string userId = GetUserId();
            try
            {
                if (string.IsNullOrEmpty(userId))
                    throw new Exception("User is not logged-in");
                var wishlist = await GetWishlist(userId);
                if (wishlist is null)
                    throw new Exception("Invalid wishlist");
                // Wishlist detail section
                var wishlistItem = _db.WishlistDetails
                    .FirstOrDefault(a => a.WishlistId == wishlist.Id && a.ProductId == productId);
                if (wishlistItem is null)
                    throw new Exception("No items in wishlist");
                _db.WishlistDetails.Remove(wishlistItem);
                _db.SaveChanges();
            }
            catch (Exception ex)
            {
                // Log the exception
            }
            var wishlistItemCount = await GetWishlistItemCount(userId);
            return wishlistItemCount;
        }


        public async Task<Wishlist> GetUserWishlist()
        {
            var userId = GetUserId();
            if (userId == null)
                throw new Exception("Invalid userid");
            var wishlist = await _db.Wishlists
                .Include(a => a.WishlistDetails)
                .ThenInclude(a => a.Product)
                .ThenInclude(a => a.Category)
                .Where(a => a.UserId == userId).FirstOrDefaultAsync();
            return wishlist;
        }

        public async Task<Wishlist> GetWishlist(string userId)
        {
            var wishlist = await _db.Wishlists.FirstOrDefaultAsync(x => x.UserId == userId);
            return wishlist;
        }

        public async Task<int> GetWishlistItemCount(string userId = "")
        {
            if (string.IsNullOrEmpty(userId))
            {
                userId = GetUserId();
            }
            var data = await (from wishlist in _db.Wishlists
                              join wishlistDetail in _db.WishlistDetails
                                  on wishlist.Id equals wishlistDetail.WishlistId
                              select new { wishlistDetail.Id }
            ).ToListAsync();
            return data.Count;
        }

        public async Task<bool> DoCheckout()
        {
            using var transaction = _db.Database.BeginTransaction();
            try
            {
                // logic
                // move data from WishlistDetail to order and order detail then we will remove Wishlist detail
                var userId = GetUserId();
                if (string.IsNullOrEmpty(userId))
                    throw new Exception("User is not logged-in");
                var wishlist = await GetWishlist(userId);
                if (wishlist is null)
                    throw new Exception("Invalid wishlist");
                var wishlistDetail = _db.WishlistDetails
                    .Where(a => a.WishlistId == wishlist.Id).ToList();
                if (wishlistDetail.Count == 0)
                    throw new Exception("Wishlist is empty");
                var order = new Order
                {
                    UserId = userId,
                    CreateDate = DateTime.UtcNow,
                    OrderStatusId = 1 // pending
                };
                _db.Orders.Add(order);
                _db.SaveChanges();
                foreach (var item in wishlistDetail)
                {
                    var orderDetail = new OrderDetail
                    {
                        ProductId = item.ProductId,
                        OrderId = order.Id,
                        Quantity = 1,
                        UnitPrice = item.UnitPrice
                    };
                    _db.OrderDetails.Add(orderDetail);
                }
                _db.SaveChanges();

                // removing the WishlistDetails
                _db.WishlistDetails.RemoveRange(wishlistDetail);
                _db.SaveChanges();
                transaction.Commit();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private string GetUserId()
        {
            var principal = _httpContextAccessor.HttpContext.User;
            string userId = _userManager.GetUserId(principal);
            return userId;
        }
    }
}
