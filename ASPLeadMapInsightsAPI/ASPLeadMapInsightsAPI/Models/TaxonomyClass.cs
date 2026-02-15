namespace ASPLeadMapInsightsAPI.Models;

/// <summary>
/// Модел за таблица Class (клас в таксономията). Референтна таблица – Tree.TaxonomyClassId сочи тук.
/// </summary>
public class TaxonomyClass
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
