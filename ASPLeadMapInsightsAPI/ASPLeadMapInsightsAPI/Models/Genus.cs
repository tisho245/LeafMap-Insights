namespace ASPLeadMapInsightsAPI.Models;

/// <summary>
/// Модел за таблица Genus (род). Референтна таблица – Tree.GenusId сочи тук.
/// </summary>
public class Genus
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
