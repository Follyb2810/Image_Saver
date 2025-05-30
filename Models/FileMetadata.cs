using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;


namespace FileStorageService.Models
{
    public class FileMetadata
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string OriginalFileName { get; set; } = string.Empty;
        public string StoredFileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long Size { get; set; }
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
        public string FileType { get; set; } = string.Empty;
        [NotMapped]
        public Dictionary<string, object> CustomMetadata { get; set; } = new();
         public string? Url { get; set; }
    }
}