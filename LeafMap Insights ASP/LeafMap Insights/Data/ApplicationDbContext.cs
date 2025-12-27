using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using LeafMap_Insights.Models;

namespace LeafMap_Insights.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Family> Families { get; set; }
        public DbSet<Genus> Genera { get; set; }
        public DbSet<Species> Species { get; set; }
        public DbSet<Tree> Trees { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure Family
            builder.Entity<Family>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Name).IsUnique();
            });

            // Configure Genus
            builder.Entity<Genus>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.LatinName).IsUnique();
                entity.HasOne(e => e.Family)
                      .WithMany(f => f.Genera)
                      .HasForeignKey(e => e.FamilyId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Species
            builder.Entity<Species>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.LatinName).IsUnique();
                entity.HasOne(e => e.Genus)
                      .WithMany(g => g.Species)
                      .HasForeignKey(e => e.GenusId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Tree
            builder.Entity<Tree>(entity =>
            {
                entity.HasKey(e => e.Id);
                // Remove unique index on QRCodeValue as it can cause issues with large Base64 strings
                entity.HasOne(e => e.Species)
                      .WithMany(s => s.Trees)
                      .HasForeignKey(e => e.SpeciesId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
