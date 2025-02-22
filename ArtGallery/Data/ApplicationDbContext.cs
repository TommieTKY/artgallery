using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ArtGallery.Models;

namespace ArtGallery.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Artwork> Artworks { get; set; }
        public DbSet<Artist> Artists { get; set; }
        public DbSet<Exhibition> Exhibitions { get; set; }
    }
}