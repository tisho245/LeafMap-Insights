namespace ASPLeadMapInsightsAPI.Models;

/// <summary>
/// Модел за таблица Division (отдел в таксономията).
/// Референтна таблица – всяко дърво (Tree) има DivisionId, който сочи тук.
/// </summary>
public class Division
{
    /// <summary>Първичен ключ – генерира се от БД.</summary>
    public int Id { get; set; }
    /// <summary>Име на отдела (напр. Magnoliophyta).</summary>
    public string Name { get; set; } = string.Empty;
}
