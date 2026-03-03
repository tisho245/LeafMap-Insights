namespace ASPLeadMapInsightsAPI.Models;

/// <summary>
/// DTO за създаване на дърво (POST api/trees). Само скаларни полета – EF не вмъква в Divisions/Families/Genera/Species/TaxonomyClasses.
/// </summary>
public class CreateTreeDto
{
    /// <summary>Id се използва само при обновяване (PUT api/trees/{id}). При създаване може да е 0.</summary>
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? PhotoURL { get; set; }
    public string? Description { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public int DivisionId { get; set; }
    public int TaxonomyClassId { get; set; }
    public int GenusId { get; set; }
    public int FamilyId { get; set; }
    public int SpeciesId { get; set; }
    public int? KlasId { get; set; }
}
