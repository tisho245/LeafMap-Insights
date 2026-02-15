namespace ASPLeadMapInsightsAPI.Models;

/// <summary>
/// Модел за таблица Species (вид). Референтна таблица – Tree.SpeciesId сочи тук.
/// </summary>
public class Species
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
