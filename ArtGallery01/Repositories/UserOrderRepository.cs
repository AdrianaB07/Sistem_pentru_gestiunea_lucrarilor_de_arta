using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

public class UserOrderRepository : IUserOrderRepository
{
    private readonly ApplicationDbContext _db;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly UserManager<IdentityUser> _userManager;

    public UserOrderRepository(ApplicationDbContext db, UserManager<IdentityUser> userManager, IHttpContextAccessor httpContextAccessor)
    {
        _db = db;
        _httpContextAccessor = httpContextAccessor;
        _userManager = userManager;
    }

    public async Task<IEnumerable<Order>> UserOrders()
    {
        var userId = GetUserId();
        if (string.IsNullOrEmpty(userId))
            throw new Exception("User is not logged-in");

        var user = await _userManager.FindByIdAsync(userId);
        var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");

        IQueryable<Order> ordersQuery = _db.Orders
            .Include(x => x.OrderStatus)
            .Include(x => x.OrderDetail)
            .ThenInclude(x => x.Product)
            .ThenInclude(x => x.Category)
            .Include(x => x.OrderDetail)
            .ThenInclude(x => x.UserOrderDetails);

        if (!isAdmin)
        {
            // Filtrare pentru a include comenzile plasate de utilizator
            ordersQuery = ordersQuery.Where(o => o.UserId == userId);
        }

        var orders = await ordersQuery.ToListAsync();

        // Preluare email-uri proprietari
        foreach (var order in orders)
        {
            foreach (var orderDetail in order.OrderDetail)
            {
                var owner = await _userManager.FindByIdAsync(orderDetail.Product.OwnerId);
                orderDetail.Product.OwnerEmail = owner?.Email;
            }
        }

        return orders;
    }

    public async Task UpdateOrderStatus(int orderId, int statusId)
    {
        var statusExists = await _db.orderStatuses.AnyAsync(s => s.Id == statusId);
        if (!statusExists)
        {
            throw new Exception("Invalid status id");
        }

        var order = await _db.Orders.FindAsync(orderId);
        if (order != null)
        {
            order.OrderStatusId = statusId;
            await _db.SaveChangesAsync();
        }
    }

    private string GetUserId()
    {
        var principal = _httpContextAccessor.HttpContext.User;
        string userId = _userManager.GetUserId(principal);
        return userId;
    }
}
