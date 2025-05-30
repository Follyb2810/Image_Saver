using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FileStorageService.Models;

namespace FileStorageService.Services
{
    public interface IFileStorageService
    {
        Task<FileUploadResult> UploadFileAsync(IFormFile file, string? folder = null);
        Task<FileUploadResult> UploadFileWithTransformationAsync(IFormFile file, FileTransformation transformation, string? folder = null);
        Task<bool> DeleteFileAsync(string publicId);
        Task<Stream?> GetFileAsync(string publicId);
        Task<FileMetadata?> GetFileMetadataAsync(string publicId);
        string GenerateUrl(string publicId, FileTransformation? transformation = null);
    }
}