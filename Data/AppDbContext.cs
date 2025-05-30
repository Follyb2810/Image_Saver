using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FileStorageService.Models;
using Microsoft.EntityFrameworkCore;

namespace FileStorageService.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

public DbSet<FileMetadata> Files { get; set; }

    }
}