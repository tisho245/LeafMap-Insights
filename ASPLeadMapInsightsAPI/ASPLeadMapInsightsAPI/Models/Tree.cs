namespace ASPLeadMapInsightsAPI.Models;

/// <summary>
/// Централна таблица Tree (дърво). Всички референтни таблици (Division, TaxonomyClass, Genus, Family, Species)
/// са свързани с нея чрез външни ключове в тази таблица.
/// </summary>
public class Tree
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    /// <summary>URL на снимка – по избор.</summary>
    public string? PhotoURL { get; set; }
    public string? Description { get; set; }
    /// <summary>GPS ширина – за показване на карта и филтър WithinBounds.</summary>
    public double Latitude { get; set; }
    /// <summary>GPS дължина – за показване на карта и филтър WithinBounds.</summary>
    public double Longitude { get; set; }

    // Външни ключове към референтните таблици (по ER диаграмата).
    public int DivisionId { get; set; }
    public int TaxonomyClassId { get; set; }
    public int GenusId { get; set; }
    public int FamilyId { get; set; }
    public int SpeciesId { get; set; }
    /// <summary>По избор – в схемата няма таблица Klas; за бъдещо разширяване.</summary>
    public int? KlasId { get; set; }

    // Навигационни свойства – EF ги използва за Include() и за релациите.
    public Division Division { get; set; } = null!;
    public TaxonomyClass TaxonomyClass { get; set; } = null!;
    public Genus Genus { get; set; } = null!;
    public Family Family { get; set; } = null!;
    public Species Species { get; set; } = null!;
}
