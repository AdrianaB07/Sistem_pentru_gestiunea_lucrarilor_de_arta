namespace ArtGallery01.Repositories
{
    public interface IUserOrderRepository
    {
        Task<IEnumerable<Order>> UserOrders();
        Task UpdateOrderStatus(int orderId, int statusId);
    }
}
