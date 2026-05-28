using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuctionApp.Controllers;

[ApiController]
[Route("api/uploads")]
[Authorize]
public class UploadsController(IWebHostEnvironment env) : ControllerBase
{
    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp"];
    private const long MaxFileSize = 5 * 1024 * 1024; // 5 MB

    [HttpPost("image")]
    public async Task<IActionResult> UploadImage(IFormFile file)
    {
        if (file.Length == 0)
            return BadRequest(new { message = "Файл порожній" });

        if (file.Length > MaxFileSize)
            return BadRequest(new { message = "Файл перевищує 5 МБ" });

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(ext))
            return BadRequest(new { message = "Дозволені формати: JPG, PNG, WebP" });

        var uploadsDir = Path.Combine(env.WebRootPath, "uploads", "lots");
        Directory.CreateDirectory(uploadsDir);

        var fileName  = $"{Guid.NewGuid()}{ext}";
        var filePath  = Path.Combine(uploadsDir, fileName);

        await using var stream = System.IO.File.Create(filePath);
        await file.CopyToAsync(stream);

        return Ok(new { path = $"/uploads/lots/{fileName}" });
    }
}
