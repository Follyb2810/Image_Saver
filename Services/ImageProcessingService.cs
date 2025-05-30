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
    public class ImageProcessingService : IImageProcessingService
    {
        public async Task<byte[]> ProcessImageAsync(IFormFile file, FileTransformation transformation)
        {
            using var stream = file.OpenReadStream();
            using var image = await Image.LoadAsync(stream);

            // Apply transformations
            if (transformation.Width.HasValue || transformation.Height.HasValue)
            {
                var resizeOptions = new ResizeOptions
                {
                    Size = new Size(transformation.Width ?? image.Width, transformation.Height ?? image.Height),
                    Mode = transformation.Crop ? GetResizeMode(transformation.CropMode) : ResizeMode.Max
                };

                image.Mutate(x => x.Resize(resizeOptions));
            }

            // Convert format and save
            using var outputStream = new MemoryStream();
            
            var format = transformation.Format?.ToLowerInvariant() ?? "jpg";
            switch (format)
            {
                case "jpg":
                case "jpeg":
                    var jpegEncoder = new JpegEncoder { Quality = transformation.Quality ?? 85 };
                    await image.SaveAsJpegAsync(outputStream, jpegEncoder);
                    break;
                case "png":
                    await image.SaveAsPngAsync(outputStream);
                    break;
                case "webp":
                    var webpEncoder = new WebpEncoder { Quality = transformation.Quality ?? 85 };
                    await image.SaveAsWebpAsync(outputStream, webpEncoder);
                    break;
                default:
                    await image.SaveAsJpegAsync(outputStream);
                    break;
            }

            return outputStream.ToArray();
        }

        public async Task<byte[]> ResizeImageAsync(byte[] imageData, int? width, int? height, bool crop = false, string cropMode = "fill")
        {
            using var image = Image.Load(imageData);
            
            var resizeOptions = new ResizeOptions
            {
                Size = new Size(width ?? image.Width, height ?? image.Height),
                Mode = crop ? GetResizeMode(cropMode) : ResizeMode.Max
            };

            image.Mutate(x => x.Resize(resizeOptions));

            using var outputStream = new MemoryStream();
            await image.SaveAsJpegAsync(outputStream);
            return outputStream.ToArray();
        }

        public async Task<byte[]> ConvertFormatAsync(byte[] imageData, string format, int? quality = null)
        {
            using var image = Image.Load(imageData);
            using var outputStream = new MemoryStream();

            switch (format.ToLowerInvariant())
            {
                case "jpg":
                case "jpeg":
                    var jpegEncoder = new JpegEncoder { Quality = quality ?? 85 };
                    await image.SaveAsJpegAsync(outputStream, jpegEncoder);
                    break;
                case "png":
                    await image.SaveAsPngAsync(outputStream);
                    break;
                case "webp":
                    var webpEncoder = new WebpEncoder { Quality = quality ?? 85 };
                    await image.SaveAsWebpAsync(outputStream, webpEncoder);
                    break;
                default:
                    await image.SaveAsJpegAsync(outputStream);
                    break;
            }

            return outputStream.ToArray();
        }

        private ResizeMode GetResizeMode(string cropMode)
        {
            return cropMode.ToLowerInvariant() switch
            {
                "fill" => ResizeMode.Crop,
                "fit" => ResizeMode.Max,
                "scale" => ResizeMode.Stretch,
                _ => ResizeMode.Crop
            };
        }
    }
}