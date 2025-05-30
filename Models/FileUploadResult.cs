using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FileStorageService.Models
{
    public class FileUploadResult
    {
          public bool Success { get; set; }
        public string? Message { get; set; }
        public string? FileName { get; set; }
        public string? OriginalFileName { get; set; }
        public string? Url { get; set; }
        public string? FileType { get; set; }
        public long FileSize { get; set; }
        public DateTime UploadedAt { get; set; }
        public string? PublicId { get; set; }
        public Dictionary<string, object>? Metadata { get; set; }
    }
}