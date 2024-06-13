using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ArtGallery01.Models;
using Microsoft.EntityFrameworkCore;

public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;

    public AdminController(ApplicationDbContext context)
    {
        _context = context;
    }

    public IActionResult Dashboard()
    {
        var model = new AdminDashboardDto
        {
            MonthlySales = GetMonthlySalesData(),
            PendingOrders = GetOrdersCountByStatus("Pending"),
            ProcessingOrders = GetOrdersCountByStatus("Processing"),
            ShippedOrders = GetOrdersCountByStatus("Shipped"),
            DeliveredOrders = GetOrdersCountByStatus("Delivered"),
            CancelledOrders = GetOrdersCountByStatus("Cancelled"),
            SalesByCategory = GetSalesByCategoryData()
        };

        return View(model);
    }

    private List<MonthlySalesData> GetMonthlySalesData()
    {
        var currentYear = DateTime.Now.Year;

        var orders = _context.Orders
            .Where(o => o.CreateDate.Year == currentYear)
            .Include(o => o.OrderDetail)
            .ToList();

        var orderDetails = orders.SelectMany(o => o.OrderDetail).Where(od => od != null && od.Order != null);

        var monthlySales = orderDetails
            .GroupBy(od => od.Order.CreateDate.Month)
            .Select(g => new MonthlySalesData
            {
                Month = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(g.Key),
                Sales = (int)g.Sum(od => od.UnitPrice * od.Quantity)
            })
            .ToList();

        // Lista cu toate lunile anului și vânzările fictive
        var allMonths = new List<MonthlySalesData>
    {
        new MonthlySalesData { Month = "January", Sales = 1500 },
        new MonthlySalesData { Month = "February", Sales = 11500 },
        new MonthlySalesData { Month = "March", Sales = 22000 },
        new MonthlySalesData { Month = "April", Sales = 1000 },
        new MonthlySalesData { Month = "May", Sales = 21500 },
        new MonthlySalesData { Month = "June", Sales = 1000 },
        new MonthlySalesData { Month = "July", Sales = 1 },
        new MonthlySalesData { Month = "August", Sales = 1 },
        new MonthlySalesData { Month = "September", Sales = 1 },
        new MonthlySalesData { Month = "October", Sales = 1 },
        new MonthlySalesData { Month = "November", Sales = 1 },
        new MonthlySalesData { Month = "December", Sales = 1 }
    };

        // Combinarea datelor reale cu cele fictive
        foreach (var monthData in monthlySales)
        {
            var month = allMonths.FirstOrDefault(m => m.Month == monthData.Month);
            if (month != null)
            {
                month.Sales = monthData.Sales; // Suprascrie valorile fictive cu cele reale
            }
        }

        return allMonths.OrderBy(msd => DateTime.ParseExact(msd.Month, "MMMM", CultureInfo.CurrentCulture).Month).ToList();
    }

    private List<SalesByCategoryData> GetSalesByCategoryData()
    {
        var salesByCategory = _context.OrderDetails
            .Include(od => od.Product)
            .ThenInclude(p => p.Category)
            .GroupBy(od => od.Product.Category.CategoryName)
            .Select(g => new SalesByCategoryData
            {
                CategoryName = g.Key,
                Sales = (int)g.Sum(od => od.UnitPrice * od.Quantity),
                ProductsSold = g.Sum(od => od.Quantity) // Numărul de produse vândute
            })
            .ToList();

        return salesByCategory;
    }


    private int GetOrdersCountByStatus(string status)
    {
        // Exemplu de numărare a comenzilor pe status - înlocuiește cu logica ta
        return _context.Orders.Count(o => o.OrderStatus.StatusName == status);
    }
}
