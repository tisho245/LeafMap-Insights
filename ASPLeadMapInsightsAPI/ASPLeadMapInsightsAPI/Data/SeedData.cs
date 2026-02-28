using ASPLeadMapInsightsAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace ASPLeadMapInsightsAPI.Data;

/// <summary>
/// Запълване на таксономичните таблици и примерни дървета при стартиране.
/// Идемпотентно: добавя само липсващи записи по име.
/// </summary>
public static class SeedData
{
    private static readonly string[] DivisionNames = { "Magnoliophyta", "Pinophyta", "Pteridophyta" };
    private static readonly string[] ClassNames = { "Magnoliopsida", "Pinopsida", "Polypodiopsida" };
    private static readonly string[] FamilyNames = { "Fagaceae", "Betulaceae", "Pinaceae", "Rosaceae", "Salicaceae", "Oleaceae", "Aceraceae" };
    private static readonly string[] GenusNames = { "Quercus", "Fagus", "Betula", "Pinus", "Picea", "Rosa", "Salix", "Fraxinus", "Acer" };
    private static readonly string[] SpeciesNames = { "Quercus robur", "Quercus petraea", "Fagus sylvatica", "Betula pendula", "Pinus sylvestris", "Picea abies", "Acer platanoides", "Fraxinus excelsior" };

    /// <summary>
    /// Добавя таксономия и примерни дървета ако липсват. Може да се вика многократно.
    /// </summary>
    public static async Task EnsureSeededAsync(LeafMapDbContext context)
    {
        await EnsureTaxonomyAsync(context);
        await EnsureSampleTreesAsync(context);
    }

    private static async Task EnsureTaxonomyAsync(LeafMapDbContext context)
    {
        foreach (var name in DivisionNames)
        {
            if (!await context.Divisions.AnyAsync(d => d.Name == name))
                context.Divisions.Add(new Division { Name = name });
        }
        foreach (var name in ClassNames)
        {
            if (!await context.TaxonomyClasses.AnyAsync(c => c.Name == name))
                context.TaxonomyClasses.Add(new TaxonomyClass { Name = name });
        }
        foreach (var name in FamilyNames)
        {
            if (!await context.Families.AnyAsync(f => f.Name == name))
                context.Families.Add(new Family { Name = name });
        }
        foreach (var name in GenusNames)
        {
            if (!await context.Genera.AnyAsync(g => g.Name == name))
                context.Genera.Add(new Genus { Name = name });
        }
        foreach (var name in SpeciesNames)
        {
            if (!await context.Species.AnyAsync(s => s.Name == name))
                context.Species.Add(new Species { Name = name });
        }
        await context.SaveChangesAsync();
    }

    private static async Task EnsureSampleTreesAsync(LeafMapDbContext context)
    {
        if (await context.Trees.AnyAsync())
            return;

        var divisionId = (await context.Divisions.OrderBy(d => d.Id).FirstAsync()).Id;
        var classId = (await context.TaxonomyClasses.OrderBy(c => c.Id).FirstAsync()).Id;
        var genusId = (await context.Genera.FirstAsync(g => g.Name == "Quercus")).Id;
        var familyId = (await context.Families.FirstAsync(f => f.Name == "Fagaceae")).Id;
        var speciesId = (await context.Species.FirstAsync(s => s.Name == "Quercus robur")).Id;

        context.Trees.AddRange(
            new Tree
            {
                Name = "Дъб в парка",
                Description = "Стар дъб в централния парк.",
                Latitude = 42.6977,
                Longitude = 23.3219,
                DivisionId = divisionId,
                TaxonomyClassId = classId,
                GenusId = genusId,
                FamilyId = familyId,
                SpeciesId = speciesId
            },
            new Tree
            {
                Name = "Втори дъб",
                Description = "По-малък дъб близо до входа.",
                Latitude = 42.6980,
                Longitude = 23.3225,
                DivisionId = divisionId,
                TaxonomyClassId = classId,
                GenusId = genusId,
                FamilyId = familyId,
                SpeciesId = speciesId
            }
        );
        await context.SaveChangesAsync();
    }
}
