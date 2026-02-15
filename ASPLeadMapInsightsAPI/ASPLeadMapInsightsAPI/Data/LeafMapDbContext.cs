using ASPLeadMapInsightsAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ASPLeadMapInsightsAPI.Data;

/// <summary>
/// Контекст за базата данни. Наследява IdentityDbContext – добавя таблици за потребители (AspNetUsers и др.)
/// и същевременно съдържа таблиците за таксономия и дървета. Един контекст = една БД.
/// </summary>
public class LeafMapDbContext : IdentityDbContext<IdentityUser>
{
    public LeafMapDbContext(DbContextOptions<LeafMapDbContext> options)
        : base(options)
    {
    }

    // DbSet = достъп до таблицата. Името на свойството става име на таблицата (напр. Divisions).
    public DbSet<Division> Divisions { get; set; }
    public DbSet<TaxonomyClass> TaxonomyClasses { get; set; }
    public DbSet<Genus> Genera { get; set; }
    public DbSet<Family> Families { get; set; }
    public DbSet<Species> Species { get; set; }
    public DbSet<Tree> Trees { get; set; }

    /// <summary>
    /// Конфигуриране на релациите. Tree има FK към всяка референтна таблица;
    /// Restrict означава: при изтриване на запис от Division/Rod и т.н. да не се изтриват автоматично дърветата.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder); // Identity таблиците се конфигурират от базовия клас.

        // Tree.DivisionId → Division.Id; едно дърво има един Division.
        modelBuilder.Entity<Tree>()
            .HasOne(t => t.Division)
            .WithMany()
            .HasForeignKey(t => t.DivisionId)
            .OnDelete(DeleteBehavior.Restrict);

        // Защо Restrict: при изтриване на Division/Species да не се каскадно изтриват дърветата; БД връща грешка и данните остават консистентни.
        modelBuilder.Entity<Tree>()
            .HasOne(t => t.TaxonomyClass)
            .WithMany()
            .HasForeignKey(t => t.TaxonomyClassId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Tree>()
            .HasOne(t => t.Genus)
            .WithMany()
            .HasForeignKey(t => t.GenusId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Tree>()
            .HasOne(t => t.Family)
            .WithMany()
            .HasForeignKey(t => t.FamilyId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Tree>()
            .HasOne(t => t.Species)
            .WithMany()
            .HasForeignKey(t => t.SpeciesId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
