using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using FileStorageService.Services;
using FileStorageService.Models;

namespace FileStorageService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FilesController : ControllerBase
    {
        private readonly IFileStorageService _fileStorageService;

        public FilesController(IFileStorageService fileStorageService)
        {
            _fileStorageService = fileStorageService;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile(IFormFile file, [FromQuery] string? folder = null)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { success = false, message = "No file provided" });
            }

            var result = await _fileStorageService.UploadFileAsync(file, folder);

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpPost("upload/transform")]
        public async Task<IActionResult> UploadFileWithTransformation(
            IFormFile file,
            [FromQuery] int? width = null,
            [FromQuery] int? height = null,
            [FromQuery] string? format = null,
            [FromQuery] int? quality = null,
            [FromQuery] bool crop = false,
            [FromQuery] string cropMode = "fill",
            [FromQuery] string? folder = null)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { success = false, message = "No file provided" });
            }

            var transformation = new FileTransformation
            {
                Width = width,
                Height = height,
                Format = format,
                Quality = quality,
                Crop = crop,
                CropMode = cropMode
            };

            var result = await _fileStorageService.UploadFileWithTransformationAsync(file, transformation, folder);

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpGet("{publicId}")]
        public async Task<IActionResult> GetFile(string publicId)
        {
            var metadata = await _fileStorageService.GetFileMetadataAsync(publicId);
            if (metadata == null)
            {
                return NotFound(new { success = false, message = "File not found" });
            }

            var fileStream = await _fileStorageService.GetFileAsync(publicId);
            if (fileStream == null)
            {
                return NotFound(new { success = false, message = "File not found" });
            }

            return File(fileStream, metadata.ContentType, metadata.OriginalFileName);
        }

        [HttpGet("{publicId}/transform")]
        public async Task<IActionResult> GetTransformedFile(
            string publicId,
            [FromQuery] int? w = null,  // width
            [FromQuery] int? h = null,  // height
            [FromQuery] string? f = null, // format
            [FromQuery] int? q = null,  // quality
            [FromQuery] string? c = null) // crop mode
        {
            var metadata = await _fileStorageService.GetFileMetadataAsync(publicId);
            if (metadata == null)
            {
                return NotFound(new { success = false, message = "File not found" });
            }

            // For now, return original file if no transformations
            // In a production system, you'd cache transformed versions
            var fileStream = await _fileStorageService.GetFileAsync(publicId);
            if (fileStream == null)
            {
                return NotFound(new { success = false, message = "File not found" });
            }

            return File(fileStream, metadata.ContentType);
        }

        [HttpDelete("{publicId}")]
        public async Task<IActionResult> DeleteFile(string publicId)
        {
            var success = await _fileStorageService.DeleteFileAsync(publicId);

            if (success)
            {
                return Ok(new { success = true, message = "File deleted successfully" });
            }

            return NotFound(new { success = false, message = "File not found or could not be deleted" });
        }

        [HttpGet("{publicId}/metadata")]
        public async Task<IActionResult> GetFileMetadata(string publicId)
        {
            var metadata = await _fileStorageService.GetFileMetadataAsync(publicId);
            if (metadata == null)
            {
                return NotFound(new { success = false, message = "File not found" });
            }

            return Ok(new { success = true, data = metadata });
        }

        [HttpGet("{publicId}/url")]
        public async Task<IActionResult> GetFileUrl(
            string publicId,
            [FromQuery] int? width = null,
            [FromQuery] int? height = null,
            [FromQuery] string? format = null,
            [FromQuery] int? quality = null,
            [FromQuery] bool crop = false,
            [FromQuery] string cropMode = "fill")
        {
            var metadata = await _fileStorageService.GetFileMetadataAsync(publicId);
            if (metadata == null)
            {
                return NotFound(new { success = false, message = "File not found" });
            }

            FileTransformation? transformation = null;
            if (width.HasValue || height.HasValue || !string.IsNullOrEmpty(format))
            {
                transformation = new FileTransformation
                {
                    Width = width,
                    Height = height,
                    Format = format,
                    Quality = quality,
                    Crop = crop,
                    CropMode = cropMode
                };
            }

            var url = _fileStorageService.GenerateUrl(publicId, transformation);
            return Ok(new { success = true, url = url });
        }
    }
}