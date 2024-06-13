namespace ArtGallery01.Models.DTOs
{
    // Models/AdminDashboardDto.cs
    public class AdminDashboardDto
    {
        public List<MonthlySalesData> MonthlySales { get; set; }
        public int PendingOrders { get; set; }
        public int ProcessingOrders { get; set; }
        public int ShippedOrders { get; set; }
        public int DeliveredOrders { get; set; }
        public int CancelledOrders { get; set; }
        public List<SalesByCategoryData> SalesByCategory { get; set; }
    }
    public class SalesByCategoryData
    {
        public string CategoryName { get; set; }
        public decimal Sales { get; set; }
        public int ProductsSold { get; set; }
    }

    public class MonthlySalesData
    {
        public string Month { get; set; }
        public int Sales { get; set; }
    }

}
