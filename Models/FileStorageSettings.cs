using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FileStorageService.Models
{
   public class FileStorageSettings
    {
        public string UploadPath { get; set; } = "uploads";
        public string BaseUrl { get; set; } = "https://localhost:7000";
        public long MaxFileSize { get; set; } = 100 * 1024 * 1024; // 100MB
        public string[] AllowedExtensions { get; set; } = 
        {
            ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp", // Images
            ".pdf", ".doc", ".docx", ".txt", ".rtf", // Documents
            ".mp4", ".avi", ".mov", ".wmv", // Videos
            ".mp3", ".wav", ".ogg" // Audio
        };
    }
}