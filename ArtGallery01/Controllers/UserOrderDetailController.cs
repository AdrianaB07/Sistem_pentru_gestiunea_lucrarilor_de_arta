using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ArtGallery01.Models;
using System.Linq;
using System.Threading.Tasks;

namespace ArtGallery01.Controllers
{
    public class UserOrderDetailController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserOrderDetailController(ApplicationDbContext context, UserManager<IdentityUser> userManager, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
        }

        // GET: UserOrderDetail/Create
        public async Task<IActionResult> Create()
        {
            var principal = _httpContextAccessor.HttpContext.User;
            string userId = _userManager.GetUserId(principal);
            var cart = await _context.ShoppingCarts
                                    .Include(c => c.CartDetails)
                                    .ThenInclude(cd => cd.Product)
                                    .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null || cart.CartDetails == null || !cart.CartDetails.Any())
            {
                // Handle the scenario where the shopping cart is empty or does not exist
                return RedirectToAction("Index", "Home");
            }

            decimal totalAmount = (decimal)cart.CartDetails.Sum(cd => cd.Product.Price * cd.Quantity);

            var model = new UserOrderDetail
            {
                TotalAmount = totalAmount,
                ShoppingCartId = cart.Id
            };

            ViewData["TotalAmount"] = totalAmount; // Pass the total amount to the view

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,FirstName,LastName,Address,email,ShoppingCartId,TotalAmount")] UserOrderDetail userOrderDetail)
        {
            if (ModelState.IsValid)
            {
                var principal = _httpContextAccessor.HttpContext.User;
                string userId = _userManager.GetUserId(principal);
                var cart = await _context.ShoppingCarts
                    .Include(c => c.CartDetails)
                    .ThenInclude(cd => cd.Product)
                    .FirstOrDefaultAsync(x => x.UserId == userId);

                if (cart == null)
                {
                    // Handle the scenario where the shopping cart is empty or does not exist
                    return RedirectToAction("Index", "Home");
                }

                // Calculate the total amount based on the shopping cart details
                decimal totalAmount = (decimal)cart.CartDetails.Sum(cd => cd.Product.Price * cd.Quantity);

                // Set the TotalAmount property in the UserOrderDetail model
                userOrderDetail.TotalAmount = totalAmount;
                userOrderDetail.ShoppingCartId = cart.Id;

                // Add the buyer information to the database
                _context.Add(userOrderDetail);
                await _context.SaveChangesAsync();

                // Redirect the user to the checkout page
                return RedirectToAction("CreateCheckoutSession", "Cart", new { amount = totalAmount });
            }

            // If the model is not valid, return to the Create view to display validation errors
            ViewData["ShoppingCartId"] = userOrderDetail.ShoppingCartId;
            ViewData["TotalAmount"] = userOrderDetail.TotalAmount; // Ensure TotalAmount is correctly set in the model
            return View(userOrderDetail);
        }
    }
}
