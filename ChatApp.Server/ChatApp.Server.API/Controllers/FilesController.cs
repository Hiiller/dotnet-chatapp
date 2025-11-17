using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using System.IO;

namespace ChatApp.Server.API.Controllers
{
    [ApiController]
    [Route("api/files")]
    public class FilesController : ControllerBase
    {
        private readonly string _uploadRoot;

        public FilesController()
        {
            // 保存到 wwwroot/uploads
            var wwwroot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            _uploadRoot = Path.Combine(wwwroot, "uploads");
            Directory.CreateDirectory(_uploadRoot);
        }

        [HttpPost]
        [RequestSizeLimit(20 * 1024 * 1024)] // 限制 20MB
        public async Task<IActionResult> Upload([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { error = "文件为空" });
            }

            var ext = Path.GetExtension(file.FileName);
            // 允许的图片扩展名
            var allowed = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".png", ".jpg", ".jpeg", ".gif", ".webp" };
            if (!allowed.Contains(ext))
            {
                return BadRequest(new { error = "仅支持图片文件: png/jpg/jpeg/gif/webp" });
            }

            var fileName = $"{Guid.NewGuid()}{ext}";
            var savePath = Path.Combine(_uploadRoot, fileName);
            await using (var stream = System.IO.File.Create(savePath))
            {
                await file.CopyToAsync(stream);
            }

            // 返回可访问的相对URL
            var url = $"/uploads/{fileName}";
            return Ok(new { url });
        }

        [HttpGet("mime")] // 可选：返回扩展名的MIME类型
        public IActionResult GetMime([FromQuery] string fileName)
        {
            var provider = new FileExtensionContentTypeProvider();
            if (provider.TryGetContentType(fileName, out var contentType))
            {
                return Ok(new { contentType });
            }
            return Ok(new { contentType = "application/octet-stream" });
        }
    }
}