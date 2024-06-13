using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using ArtGallery01.Models;
using System.IO;

namespace ArtGallery01.Controllers
{
    [ApiController]
    [Route("api/tags")]
    public class TagsController : ControllerBase
    {
        private readonly ILogger<TagsController> _logger;
        private readonly ProductController _productController;

        public TagsController(ILogger<TagsController> logger, ProductController productController)
        {
            _logger = logger;
            _productController = productController;
        }

        [HttpPost("generate")]
        public async Task<IActionResult> GenerateTags(IFormFile image)
        {
            if (image == null || image.Length == 0)
            {
                _logger.LogError("No image file found");
                return BadRequest(new { error = "No image file found" });
            }

            // Salvează fișierul temporar
            var filePath = Path.GetTempFileName();
            try
            {
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(stream);
                }

                // Apelează metoda GetGeneratedTags din ProductController
                var tags = await _productController.GetGeneratedTags(filePath);
                if (tags == null || !tags.Any())
                {
                    _logger.LogInformation("No tags found");
                    return Ok(new { tags = new List<string>(), message = "No tags found" });
                }

                return Ok(new { tags });
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error generating tags");
                return StatusCode(500, new { error = "Error generating tags" });
            }
        }
    }
}
