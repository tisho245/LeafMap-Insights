namespace MAUILeafMapInsights.Models;

/// <summary>DTO за дърво – съответства на API Tree. Използва се при GetTrees, GetTree, CreateTree.</summary>
public class TreeDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string? PhotoURL { get; set; }
    public string? Description { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public int DivisionId { get; set; }
    public int TaxonomyClassId { get; set; }
    public int GenusId { get; set; }
    public int FamilyId { get; set; }
    public int SpeciesId { get; set; }
    public DivisionDto? Division { get; set; }
    public TaxonomyClassDto? TaxonomyClass { get; set; }
    public GenusDto? Genus { get; set; }
    public FamilyDto? Family { get; set; }
    public SpeciesDto? Species { get; set; }
}

/// <summary>Референтен DTO – от api/divisions.</summary>
public class DivisionDto { public int Id { get; set; } public string Name { get; set; } = ""; }
/// <summary>Референтен DTO – от api/taxonomyclasses.</summary>
public class TaxonomyClassDto { public int Id { get; set; } public string Name { get; set; } = ""; }
/// <summary>Референтен DTO – от api/genera.</summary>
public class GenusDto { public int Id { get; set; } public string Name { get; set; } = ""; }
/// <summary>Референтен DTO – от api/families.</summary>
public class FamilyDto { public int Id { get; set; } public string Name { get; set; } = ""; }
/// <summary>Референтен DTO – от api/species.</summary>
public class SpeciesDto { public int Id { get; set; } public string Name { get; set; } = ""; }
