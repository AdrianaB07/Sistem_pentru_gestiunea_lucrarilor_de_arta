using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArtGallery01.Controllers
{
    [Authorize]
    public class WishlistController : Controller
    {
        private readonly IWishlistRepository _wishlistRepo;

        public WishlistController(IWishlistRepository wishlistRepo)
        {
            _wishlistRepo = wishlistRepo;
        }

        public async Task<IActionResult> AddItem(int productId, int qty = 1, int redirect = 0)
        {
            var wishlistItemCount = await _wishlistRepo.AddItem(productId, qty);
            if (redirect == 0)
                return Json(new { success = true, message = "Product added to wishlist successfully!", wishlistItemCount });
            return RedirectToAction("GetUserWishlist");
        }

        public async Task<IActionResult> RemoveItem(int productId)
        {
            var wishlistItemCount = await _wishlistRepo.RemoveItem(productId);
            return RedirectToAction("GetUserWishlist");
        }

        public async Task<IActionResult> GetUserWishlist()
        {
            var wishlist = await _wishlistRepo.GetUserWishlist();
            return View(wishlist);
        }

        public async Task<IActionResult> GetTotalItemInWishlist()
        {
            int wishlistItemCount = await _wishlistRepo.GetWishlistItemCount();
            return Ok(wishlistItemCount);
        }

        public async Task<IActionResult> Checkout()
        {
            bool isCheckedOut = await _wishlistRepo.DoCheckout();
            if (!isCheckedOut)
                throw new Exception("Something happened on the server side");
            return RedirectToAction("Index", "Home");
        }
    }
}
