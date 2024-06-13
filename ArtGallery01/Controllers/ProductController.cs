using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ArtGallery01.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace ArtGallery01.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly string rootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<ProductController> _logger;

        public ProductController(ApplicationDbContext context, IWebHostEnvironment environment, ILogger<ProductController> logger)
        {
            _context = context;
            _environment = environment;
            _logger = logger;
        }

        // GET: Product
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Products.Include(p => p.Category);
            return RedirectToAction("ListProducts");
        }

        // GET: Product/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Products == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Comments).ThenInclude(c => c.User)
                .Include(p => p.Tags)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            var owner = await _context.Users.FirstOrDefaultAsync(u => u.Id == product.OwnerId);
            if (owner != null)
            {
                product.OwnerEmail = owner.Email;
            }

            return View(product);
        }


        // GET: Product/Create
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "CategoryName");
            return View();
        }

        // POST: Product/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ProductName,Price,Description,CategoryId")] Product product, IFormFile uploadedImage, List<string> Tags)
        {
            if (ModelState.IsValid)
            {
                product.OwnerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                product.Tags = Tags.Select(tag => new Tag { Content = tag }).ToList();

                if (uploadedImage != null && uploadedImage.Length > 0)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(uploadedImage.FileName);
                    var filePath = Path.Combine(rootPath, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await uploadedImage.CopyToAsync(stream);
                    }

                    product.Image = fileName;

                    
                }

                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "CategoryName", product.CategoryId);
            return View(product);
        }

        // GET: Product/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Products == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Tags)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "CategoryName", product.CategoryId);
            ViewBag.Tags = string.Join(",", product.Tags.Select(t => t.Content));
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ProductName,Price,Description,CategoryId,Image")] Product product, IFormFile uploadedImage, List<string> Tags, string existingImagePath)
        {
            if (id != product.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (uploadedImage != null && uploadedImage.Length > 0)
                    {
                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(uploadedImage.FileName);
                        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await uploadedImage.CopyToAsync(stream);
                        }
                        product.Image = fileName; // Actualizează câmpul Image
                    }
                    else
                    {
                        product.Image = existingImagePath; // Păstrează imaginea existentă
                    }

                    var existingTags = await _context.Tags.Where(t => t.ProductId == product.Id).ToListAsync();
                    _context.Tags.RemoveRange(existingTags);
                    product.Tags = Tags.Select(tag => new Tag { Content = tag, ProductId = product.Id }).ToList();

                    _context.Update(product);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "CategoryName", product.CategoryId);
            return View(product);
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }



        // GET: Product/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Products == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products
                .Include(p => p.Tags)
                .Include(p => p.OrderDetail)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            // Șterge toate comenzile asociate acestui produs
            var ordersToDelete = await _context.Orders
                .Where(o => o.OrderDetail.Any(od => od.ProductId == id))
                .ToListAsync();
            _context.Orders.RemoveRange(ordersToDelete);

            // Șterge toate comentariile asociate acestui produs
            var comments = await _context.Comments.Where(c => c.ProductId == id).ToListAsync();
            _context.Comments.RemoveRange(comments);

            // Șterge imaginea asociată produsului
            if (!string.IsNullOrEmpty(product.Image))
            {
                var imagePath = Path.Combine(rootPath, product.Image);
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }

            // Șterge produsul și etichetele asociate
            _context.Tags.RemoveRange(product.Tags);
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> AddComment(int id, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                // Tratarea cazului în care conținutul comentariului este gol
                return RedirectToAction("Details", new { id = id });
            }

            var product = await _context.Products.Include(p => p.Comments).FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            var comment = new Comment
            {
                Content = content,
                DateAdded = DateTime.Now,
                UserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value, // ID-ul utilizatorului curent
                ProductId = id
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteComment(int commentId, int productId)
        {
            var comment = await _context.Comments.FindAsync(commentId);

            if (comment == null)
            {
                return NotFound();
            }

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = productId });
        }

        public async Task<IActionResult> Gallery()
        {
            var featuredProducts = await _context.Products.Where(p => p.IsFeatured).ToListAsync();
            return View(featuredProducts);
        }

        // GET: Product/Featured
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Featured()
        {
            var products = await _context.Products.ToListAsync();
            return View(products);
        }

        // POST: Product/UpdateFeatured
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateFeatured(string featuredIdsString)
        {
            // Afișăm valorile primite pentru investigare
            Console.WriteLine("FeaturedIdsString received: " + featuredIdsString);

            if (string.IsNullOrEmpty(featuredIdsString))
            {
                // Poți gestiona această situație, de exemplu, să întorci o eroare sau să redirecționezi către o altă acțiune
                return RedirectToAction(nameof(Featured));
            }

            var featuredIds = featuredIdsString.Split(',').Select(int.Parse).ToArray();

            var products = await _context.Products.ToListAsync();

            foreach (var product in products)
            {
                product.IsFeatured = featuredIds.Contains(product.Id);
            }

            await _context.SaveChangesAsync();

            // Redirecționare către acțiunea Gallery pentru a afișa produsele actualizate
            return RedirectToAction("Gallery", "Product");
        }

        public async Task<IActionResult> ArtistProducts(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return NotFound();
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                return NotFound();
            }

            var products = await _context.Products
                .Where(p => p.OwnerId == user.Id)
                .Include(p => p.Category)
                .Include(p => p.Tags)
                .ToListAsync();

            return View(products);
        }

        // GET: Product/ListProducts
        [Authorize]
        public IActionResult ListProducts()
        {
            if (User.Identity.IsAuthenticated)
            {
                if (User.IsInRole("Admin"))
                {
                    var products = _context.Products
                        .Include(p => p.Category)
                        .Include(p => p.Tags)
                        .ToList();

                    foreach (var product in products)
                    {
                        var owner = _context.Users.FirstOrDefault(u => u.Id == product.OwnerId);
                        if (owner != null)
                        {
                            product.OwnerEmail = owner.Email;
                        }
                    }

                    return View(products);
                }
                else
                {
                    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    var products = _context.Products
                        .Where(p => p.OwnerId == userId)
                        .Include(p => p.Category)
                        .Include(p => p.Tags)
                        .ToList();
                    return View(products);
                }
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }
        }

        public async Task<List<string>> GetGeneratedTags(string imagePath)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    using (var form = new MultipartFormDataContent())
                    {
                        using (var fs = System.IO.File.OpenRead(imagePath))
                        {
                            var streamContent = new StreamContent(fs);
                            streamContent.Headers.ContentType = new MediaTypeHeaderValue("multipart/form-data");
                            form.Add(streamContent, "image", Path.GetFileName(imagePath));
                            var response = await client.PostAsync("http://localhost:5000/detect", form);
                            response.EnsureSuccessStatusCode();
                            var jsonResponse = await response.Content.ReadAsStringAsync();
                            var result = Newtonsoft.Json.JsonConvert.DeserializeObject<TagResponse>(jsonResponse);
                            return result.Tags;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error communicating with the detection service");
                throw new Exception("Error communicating with the detection service", ex);
            }
        }
    }
}
