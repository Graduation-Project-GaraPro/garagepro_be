using Dtos.FileUploads;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.Cloudinaries;

namespace Garage_pro_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImageUploadController : ControllerBase
    {
        private readonly ICloudinaryService _cloudinaryService;

        public ImageUploadController(ICloudinaryService cloudinaryService)
        {
            _cloudinaryService = cloudinaryService;
        }

        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadImage([FromForm] FileUploadDto dto)
        {
            if (dto.File == null || dto.File.Length == 0)
                return BadRequest("No file provided.");

            try
            {
                var imageUrl = await _cloudinaryService.UploadImageAsync(dto.File);
                return Ok(new { imageUrl });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("upload-multiple")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadImages([FromForm] MultipleFileUploadDto dto)
        {
            if (dto.Files == null || !dto.Files.Any())
                return BadRequest("No files provided.");

            try
            {
                var imageUrls = await _cloudinaryService.UploadImagesAsync(dto.Files);
                return Ok(new { imageUrls });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
