using ImageMagick;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DagoniteEmpire.Service;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PortraitUploadController : ControllerBase
{
    private const int MaxUploadBytes = 15 * 1024 * 1024;
    private const int MaxImageDimension = 2048;
    private const int MaxIconDimension = 256;
    private const int WebpQuality = 90;

    private static readonly HashSet<string> AllowedFolders = new(StringComparer.OrdinalIgnoreCase)
    {
        "portraits",
        "icons",
    };

    private readonly IWebHostEnvironment _environment;

    public PortraitUploadController(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    [HttpPost]
    [RequestSizeLimit(MaxUploadBytes)]
    public async Task<IActionResult> Upload(IFormFile file, [FromQuery] string folder = "portraits")
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("No file provided.");
        }

        if (file.Length > MaxUploadBytes)
        {
            return BadRequest($"File exceeds the {MaxUploadBytes / (1024 * 1024)} MB limit.");
        }

        if (!AllowedFolders.Contains(folder))
        {
            return BadRequest("Invalid upload folder.");
        }

        try
        {
            var fileName = Guid.NewGuid().ToString() + ".webp";
            var folderDirectory = Path.Combine(_environment.WebRootPath, "upload", folder);
            Directory.CreateDirectory(folderDirectory);
            var filePath = Path.Combine(folderDirectory, fileName);

            await using var input = new MemoryStream();
            await file.CopyToAsync(input);
            input.Position = 0;

            using (var image = new MagickImage(input))
            {
                image.Format = MagickFormat.WebP;
                image.Quality = WebpQuality;
                var maxDimension = string.Equals(folder, "icons", StringComparison.OrdinalIgnoreCase)
                    ? MaxIconDimension
                    : MaxImageDimension;
                if (image.Width > maxDimension || image.Height > maxDimension)
                {
                    image.Resize((uint)maxDimension, (uint)maxDimension);
                }

                await image.WriteAsync(filePath);
            }

            return Ok(new { url = $"/upload/{folder}/{fileName}" });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }
}
