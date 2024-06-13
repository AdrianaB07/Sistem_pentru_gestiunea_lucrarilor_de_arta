using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
public class UserOrderController : Controller
{
    private readonly IUserOrderRepository _userOrderRepo;

    public UserOrderController(IUserOrderRepository userOrderRepo)
    {
        _userOrderRepo = userOrderRepo;
    }

    public async Task<IActionResult> UserOrders(string sortOrder, int? orderId)
    {
        var orders = await _userOrderRepo.UserOrders();

        if (orderId.HasValue)
        {
            orders = orders.Where(o => o.Id == orderId.Value);
            if (!orders.Any())
            {
                ViewBag.Message = "Order not found.";
            }
        }
        else if (!string.IsNullOrEmpty(sortOrder))
        {
            orders = orders.Where(o => o.OrderStatus.StatusName == sortOrder);
        }

        return View(orders);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateOrderStatus(int orderId, int statusId)
    {
        try
        {
            await _userOrderRepo.UpdateOrderStatus(orderId, statusId);
        }
        catch (Exception ex)
        {
            // Log error and handle it appropriately
            ModelState.AddModelError(string.Empty, ex.Message);
        }

        return RedirectToAction("UserOrders");
    }
}
