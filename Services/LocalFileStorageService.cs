using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FileStorageService.Models;
using FileStorageService.Data;
using Microsoft.Extensions.Options;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace FileStorageService.Services
{
    public class LocalFileStorageService : IFileStorageService
    {
        private readonly FileStorageSettings _settings;
        private readonly IImageProcessingService _imageProcessing;
        private readonly string _metadataPath;
        private readonly AppDbContext _dbContext;

        public LocalFileStorageService(IOptions<FileStorageSettings> settings, IImageProcessingService imageProcessing,
        AppDbContext dbContext
        )
        {
            _settings = settings.Value;
            _imageProcessing = imageProcessing;
            _metadataPath = Path.Combine(_settings.UploadPath, "metadata");

            // Ensure directories exist
            Directory.CreateDirectory(_settings.UploadPath);
            Directory.CreateDirectory(_metadataPath);
            _dbContext = dbContext;
        }

        public async Task<FileUploadResult> UploadFileAsync(IFormFile file, string? folder = null)
        {
            try
            {
                if (!IsValidFile(file))
                {
                    return new FileUploadResult
                    {
                        Success = false,
                        Message = "Invalid file type or size"
                    };
                }

                var publicId = GeneratePublicId();
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                var fileName = $"{publicId}{extension}";

                var folderPath = string.IsNullOrEmpty(folder) ? _settings.UploadPath : Path.Combine(_settings.UploadPath, folder);
                Directory.CreateDirectory(folderPath);

                var filePath = Path.Combine(folderPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var metadata = new FileMetadata
                {
                    Id = publicId,
                    OriginalFileName = file.FileName,
                    StoredFileName = fileName,
                    ContentType = file.ContentType,
                    Size = file.Length,
                    FileType = GetFileType(extension)
                };

                await SaveMetadataAsync(metadata);

                return new FileUploadResult
                {
                    Success = true,
                    FileName = fileName,
                    OriginalFileName = file.FileName,
                    Url = GenerateUrl(publicId),
                    FileType = metadata.FileType,
                    FileSize = file.Length,
                    UploadedAt = DateTime.UtcNow,
                    PublicId = publicId,
                    Message = "File uploaded successfully"
                };
            }
            catch (Exception ex)
            {
                return new FileUploadResult
                {
                    Success = false,
                    Message = $"Upload failed: {ex.Message}"
                };
            }
        }

        public async Task<FileUploadResult> UploadFileWithTransformationAsync(IFormFile file, FileTransformation transformation, string? folder = null)
        {
            if (!IsImageFile(file))
            {
                return await UploadFileAsync(file, folder); // Fallback for non-images
            }

            try
            {
                var publicId = GeneratePublicId();
                var processedImage = await _imageProcessing.ProcessImageAsync(file, transformation);

                var fileName = $"{publicId}.{transformation.Format ?? "jpg"}";
                var folderPath = string.IsNullOrEmpty(folder) ? _settings.UploadPath : Path.Combine(_settings.UploadPath, folder);
                Directory.CreateDirectory(folderPath);

                var filePath = Path.Combine(folderPath, fileName);
                await File.WriteAllBytesAsync(filePath, processedImage);

                var metadata = new FileMetadata
                {
                    Id = publicId,
                    OriginalFileName = file.FileName,
                    StoredFileName = fileName,
                    ContentType = $"image/{transformation.Format ?? "jpeg"}",
                    Size = processedImage.Length,
                    FileType = "image"
                };

                await SaveMetadataAsync(metadata);

                return new FileUploadResult
                {
                    Success = true,
                    FileName = fileName,
                    OriginalFileName = file.FileName,
                    Url = GenerateUrl(publicId),
                    FileType = "image",
                    FileSize = processedImage.Length,
                    UploadedAt = DateTime.UtcNow,
                    PublicId = publicId,
                    Message = "File uploaded and processed successfully"
                };
            }
            catch (Exception ex)
            {
                return new FileUploadResult
                {
                    Success = false,
                    Message = $"Upload with transformation failed: {ex.Message}"
                };
            }
        }

        public async Task<bool> DeleteFileAsync(string publicId)
        {
            try
            {
                var metadata = await GetFileMetadataAsync(publicId);
                if (metadata == null) return false;

                var filePath = Path.Combine(_settings.UploadPath, metadata.StoredFileName);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                var metadataFile = Path.Combine(_metadataPath, $"{publicId}.json");
                if (File.Exists(metadataFile))
                {
                    File.Delete(metadataFile);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<Stream?> GetFileAsync(string publicId)
        {
            var metadata = await GetFileMetadataAsync(publicId);
            if (metadata == null) return null;

            var filePath = Path.Combine(_settings.UploadPath, metadata.StoredFileName);
            return File.Exists(filePath) ? File.OpenRead(filePath) : null;
        }

        // public async Task<FileMetadata?> GetFileMetadataAsync(string publicId)
        // {
        //     try
        //     {
        //         var metadataFile = Path.Combine(_metadataPath, $"{publicId}.json");
        //         if (!File.Exists(metadataFile)) return null;

        //         var json = await File.ReadAllTextAsync(metadataFile);
        //         return JsonSerializer.Deserialize<FileMetadata>(json);
        //     }
        //     catch
        //     {
        //         return null;
        //     }
        // }
        public async Task<FileMetadata?> GetFileMetadataAsync(string publicId)
        {
            return await _dbContext.Files.FirstOrDefaultAsync(f => f.Id == publicId);
        }


        public string GenerateUrl(string publicId, FileTransformation? transformation = null)
        {
            var baseUrl = _settings.BaseUrl.TrimEnd('/');

            if (transformation != null)
            {
                var transformParams = new List<string>();
                if (transformation.Width.HasValue) transformParams.Add($"w_{transformation.Width}");
                if (transformation.Height.HasValue) transformParams.Add($"h_{transformation.Height}");
                if (!string.IsNullOrEmpty(transformation.Format)) transformParams.Add($"f_{transformation.Format}");
                if (transformation.Quality.HasValue) transformParams.Add($"q_{transformation.Quality}");
                if (transformation.Crop) transformParams.Add($"c_{transformation.CropMode}");

                if (transformParams.Any())
                {
                    return $"{baseUrl}/api/files/{publicId}/transform?{string.Join("&", transformParams)}";
                }
            }

            return $"{baseUrl}/api/files/{publicId}";
        }

        private async Task SaveMetadataAsync(FileMetadata metadata)
        {
            metadata.Url = GenerateUrl(metadata.Id);
            _dbContext.Files.Add(metadata);
            await _dbContext.SaveChangesAsync();
            // var metadataFile = Path.Combine(_metadataPath, $"{metadata.Id}.json");
            // var json = JsonSerializer.Serialize(metadata, new JsonSerializerOptions { WriteIndented = true });
            // await File.WriteAllTextAsync(metadataFile, json);
            //   var imageUrl = $"/images/{uniqueFileName}";

            // Save `imageUrl` to your database
            // Example:
            // var imageRecord = new Image { Url = imageUrl };
            // _dbContext.Images.Add(imageRecord);
            // await _dbContext.SaveChangesAsync();
        }

        private bool IsValidFile(IFormFile file)
        {
            if (file.Length > _settings.MaxFileSize) return false;

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            return _settings.AllowedExtensions.Contains(extension);
        }

        private bool IsImageFile(IFormFile file)
        {
            var imageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            return imageExtensions.Contains(extension);
        }

        private string GetFileType(string extension)
        {
            return extension.ToLowerInvariant() switch
            {
                ".jpg" or ".jpeg" or ".png" or ".gif" or ".bmp" or ".webp" => "image",
                ".pdf" or ".doc" or ".docx" or ".txt" or ".rtf" => "document",
                ".mp4" or ".avi" or ".mov" or ".wmv" => "video",
                ".mp3" or ".wav" or ".ogg" => "audio",
                _ => "other"
            };
        }

        private string GeneratePublicId()
        {
            return Guid.NewGuid().ToString("N")[..12]; // 12 character unique ID
        }


    }
}