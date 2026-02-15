namespace ASPLeafMapInsightsWebMVC.ViewModels;

/// <summary>ViewModel за дърво – съответства на API модела Tree. Използва се в Trees/Index, Details, Create и Map.</summary>
public class TreeVm
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
    public DivisionVm? Division { get; set; }
    public TaxonomyClassVm? TaxonomyClass { get; set; }
    public GenusVm? Genus { get; set; }
    public FamilyVm? Family { get; set; }
    public SpeciesVm? Species { get; set; }
}
