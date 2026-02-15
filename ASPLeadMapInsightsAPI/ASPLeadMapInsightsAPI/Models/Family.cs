namespace ASPLeadMapInsightsAPI.Models;

/// <summary>
/// Модел за таблица Family (семейство в таксономията). Референтна таблица – Tree.FamilyId сочи тук.
/// </summary>
public class Family
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
