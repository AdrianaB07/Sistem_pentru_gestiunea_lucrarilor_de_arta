using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;
using Stripe.Issuing;
using System.Diagnostics;

namespace ArtGallery01.Controllers
{
    public class CartController : Controller
    {
        private readonly ICartRepository _cartRepo;
        private readonly StripeSettings _stripeSettings;
        private readonly ApplicationDbContext _context;

        public CartController(ICartRepository cartRepo, IOptions<StripeSettings> stripeSettings, ApplicationDbContext context)
        {
            _cartRepo = cartRepo;
            _stripeSettings = stripeSettings.Value;
            _context = context;
        }

        public async Task<IActionResult> AddItem(int productId, int qty = 1, int redirect = 0)
        {
            var cartCount = await _cartRepo.AddItem(productId, qty);
            if (redirect == 0)
            {
                return Json(new { success = true, message = "Product added to cart successfully!", cartCount });
            }
            return RedirectToAction("GetUserCart");
        }


        public async Task<IActionResult> RemoveItem(int productId)
        {
            var cartCount = await _cartRepo.RemoveItem(productId);
            return RedirectToAction("GetUserCart");
        }

        public async Task<IActionResult> GetUserCart()
        {
            var cart = await _cartRepo.GetUserCart();

            if (cart == null || cart.CartDetails == null)
            {
                ViewData["TotalAmount"] = 0;
                return View(new ShoppingCart());
            }

            ViewData["TotalAmount"] = cart.CartDetails.Select(item => item.Product.Price * item.Quantity).Sum();
            return View(cart);
        }

        public async Task<IActionResult> GetTotalItemInCart()
        {
            int cartItem = await _cartRepo.GetCartItemCount();
            return Ok(cartItem);
        }

        public async Task<IActionResult> Checkout()
        {
            bool isCheckedOut = await _cartRepo.DoCheckout();
            if (!isCheckedOut)
                throw new Exception("Something happen in server side");
            return View();
        }

        public async Task<IActionResult> CreateCheckoutSession(string amount)
        {
            var currency = "eur"; // Currency code
            var successUrl = Url.Action("Success", "Cart", null, Request.Scheme);
            var cancelUrl = Url.Action("Cancel", "Cart", null, Request.Scheme);
            StripeConfiguration.ApiKey = _stripeSettings.SecretKey;

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string>
                {
                    "card"
                },
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            Currency = currency,
                            UnitAmount = Convert.ToInt32(amount) * 100,
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = "Product Name",
                                Description = "Product Description"
                            }
                        },
                        Quantity = 1
                    }
                },
                Mode = "payment",
                SuccessUrl = successUrl,
                CancelUrl = cancelUrl
            };

            var service = new SessionService();
            var session = service.Create(options);

            return Redirect(session.Url);
        }

        public async Task<IActionResult> Success()
        {
            var isCheckedOut = await _cartRepo.DoCheckout();
            if (!isCheckedOut)
            {
                // Tratați eroarea în cazul în care nu s-a putut efectua checkout-ul
                throw new Exception("Something happened during checkout");
            }

            return View("GetUserCart");
        }

        public IActionResult Cancel()
        {
            return RedirectToAction("Create", "UserOrderDetail");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
