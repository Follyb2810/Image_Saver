using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FileStorageService.Models
{
    public class FileTransformation
    {
        public int? Width { get; set; }
        public int? Height { get; set; }
        public string? Format { get; set; }
        public int? Quality { get; set; }
        public bool Crop { get; set; } = false;
        public string CropMode { get; set; } = "fill"; 
    }
}