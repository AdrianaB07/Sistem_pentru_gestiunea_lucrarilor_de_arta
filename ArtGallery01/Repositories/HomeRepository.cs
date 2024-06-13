using Microsoft.EntityFrameworkCore;

namespace ArtGallery01.Repositories
{
    public class HomeRepository : IHomeRepository
    {
        private readonly ApplicationDbContext _db;

        public HomeRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<Category>> Categories()
        {
            return await _db.Categories.ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetProducts(string sTerm = "", int categoryId = 0)
        {
            sTerm = sTerm.ToLower();
            IEnumerable<Product> products = await (from product in _db.Products
                                                   join category in _db.Categories
                                                   on product.CategoryId equals category.Id
                                                   where string.IsNullOrWhiteSpace(sTerm) ||
                                                         (product != null && product.ProductName.ToLower().Contains(sTerm)) ||
                                                         (product.Tags != null && product.Tags.Any(t => t.Content.ToLower().Contains(sTerm)))
                                                   select new Product
                                                   {
                                                       Id = product.Id,
                                                       Image = product.Image,
                                                       ProductName = product.ProductName,
                                                       CategoryId = product.CategoryId,
                                                       Price = product.Price,
                                                       Description = product.Description,
                                                       CategoryName = category.CategoryName,
                                                       IsSold = product.IsSold
                                                   }
                             ).ToListAsync();

            // Pentru fiecare produs, obținem tag-urile asociate
            foreach (var product in products)
            {
                product.Tags = await _db.Tags.Where(t => t.ProductId == product.Id).ToListAsync();
            }

            if (categoryId > 0)
            {
                products = products.Where(a => a.CategoryId == categoryId).ToList();
            }

            return products.OrderByDescending(p => p.Id); // Sortare după Id descrescător
        }

    }
}
