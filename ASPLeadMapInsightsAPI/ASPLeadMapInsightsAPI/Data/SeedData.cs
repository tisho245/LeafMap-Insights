using ASPLeadMapInsightsAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace ASPLeadMapInsightsAPI.Data;

/// <summary>
/// Първоначално запълване на БД при първо стартиране. Добавя по един запис във всяка референтна таблица
/// и два примера за дървета, за да може приложението да работи веднага след миграциите.
/// </summary>
public static class SeedData
{
    /// <summary>
    /// Ако Divisions вече има записи – нищо не прави (идемпотентно). Иначе добавя референтни данни и два Tree.
    /// </summary>
    public static async Task EnsureSeededAsync(LeafMapDbContext context)
    {
        if (await context.Divisions.AnyAsync())
            return;

        // Един запис във всяка референтна таблица – достатъчно за тест и демо.
        context.Divisions.Add(new Division { Name = "Magnoliophyta" });
        context.TaxonomyClasses.Add(new TaxonomyClass { Name = "Magnoliopsida" });
        context.Genera.Add(new Genus { Name = "Quercus" });
        context.Families.Add(new Family { Name = "Fagaceae" });
        context.Species.Add(new Species { Name = "Quercus robur" });
        await context.SaveChangesAsync();

        // Взимаме Id-тата на току-що добавените записи за да ги свържем с Tree.
        var divisionId = (await context.Divisions.FirstAsync()).Id;
        var classId = (await context.TaxonomyClasses.FirstAsync()).Id;
        var genusId = (await context.Genera.FirstAsync()).Id;
        var familyId = (await context.Families.FirstAsync()).Id;
        var speciesId = (await context.Species.FirstAsync()).Id;

        // Два примерни дървета с координати в София – за карта и тестове.
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
