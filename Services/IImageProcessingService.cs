using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FileStorageService.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Webp;


namespace FileStorageService.Services
{
    public interface IImageProcessingService
    {
        Task<byte[]> ProcessImageAsync(IFormFile file, FileTransformation transformation);
        Task<byte[]> ResizeImageAsync(byte[] imageData, int? width, int? height, bool crop = false, string cropMode = "fill");
        Task<byte[]> ConvertFormatAsync(byte[] imageData, string format, int? quality = null);
    }
}