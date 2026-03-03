using System.Text.Json;
using ASPLeadMapInsightsAPI.Data;
using ASPLeadMapInsightsAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace TaxonomyImport;

/// <summary>
/// Скрипт за внасяне на таксономични данни от JSON файл в БД на LeafMap Insights.
/// Използване: dotnet run [път_към_json]
/// По подразбиране чете от ../ASPLeadMapInsightsAPI/Data/taxonomic_import.json
/// </summary>
class Program
{
    static async Task<int> Main(string[] args)
    {
        var jsonPath = args.Length > 0
            ? args[0]
            : Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "ASPLeadMapInsightsAPI", "Data", "taxonomic_import.json");

        if (!Path.IsPathRooted(jsonPath))
            jsonPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, jsonPath));

        if (!File.Exists(jsonPath))
        {
            Console.WriteLine($"Файлът не е намерен: {jsonPath}");
            Console.WriteLine("Употреба: dotnet run [път_към_taxonomic_import.json]");
            return 1;
        }

        // Connection string – от appsettings на API или от променлива на средата
        var conn = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            ?? "Server=localhost,1433;Database=LeafMapInsights;User Id=sa;Password=SuperAdmin2026;TrustServerCertificate=True;";

        var appsettingsPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "ASPLeadMapInsightsAPI", "appsettings.json");
        if (File.Exists(appsettingsPath))
        {
            var json = await File.ReadAllTextAsync(appsettingsPath);
            var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("ConnectionStrings", out var cs) && cs.TryGetProperty("DefaultConnection", out var def))
                conn = def.GetString() ?? conn;
        }

        var options = new DbContextOptionsBuilder<LeafMapDbContext>()
            .UseSqlServer(conn)
            .Options;

        await using var context = new LeafMapDbContext(options);

        Console.WriteLine("Свързване с БД...");
        await context.Database.MigrateAsync();

        var jsonContent = await File.ReadAllTextAsync(jsonPath);
        var jsonOpt = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var data = JsonSerializer.Deserialize<TaxonomicImportFile>(jsonContent, jsonOpt);
        if (data?.Species == null || data.Species.Count == 0)
        {
            Console.WriteLine("В JSON няма масив 'species' или е празен.");
            return 1;
        }

        var divisions = data.Species.Select(s => s.Division).Where(s => !string.IsNullOrWhiteSpace(s)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        var classes = data.Species.Select(s => s.TaxonomyClass).Where(s => !string.IsNullOrWhiteSpace(s)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        var families = data.Species.Select(s => s.Family).Where(s => !string.IsNullOrWhiteSpace(s)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        var genera = data.Species.Select(s => s.Genus).Where(s => !string.IsNullOrWhiteSpace(s)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        var speciesNames = data.Species.Select(s => s.Species).Where(s => !string.IsNullOrWhiteSpace(s)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();

        int addedDiv = 0, addedClass = 0, addedFamily = 0, addedGenus = 0, addedSpecies = 0;

        foreach (var name in divisions)
        {
            if (!await context.Divisions.AnyAsync(d => d.Name == name))
            {
                context.Divisions.Add(new Division { Name = name });
                addedDiv++;
            }
        }

        foreach (var name in classes)
        {
            if (!await context.TaxonomyClasses.AnyAsync(c => c.Name == name))
            {
                context.TaxonomyClasses.Add(new TaxonomyClass { Name = name });
                addedClass++;
            }
        }

        foreach (var name in families)
        {
            if (!await context.Families.AnyAsync(f => f.Name == name))
            {
                context.Families.Add(new Family { Name = name });
                addedFamily++;
            }
        }

        foreach (var name in genera)
        {
            if (!await context.Genera.AnyAsync(g => g.Name == name))
            {
                context.Genera.Add(new Genus { Name = name });
                addedGenus++;
            }
        }

        await context.SaveChangesAsync();

        foreach (var name in speciesNames)
        {
            if (!await context.Species.AnyAsync(s => s.Name == name))
            {
                context.Species.Add(new Species { Name = name });
                addedSpecies++;
            }
        }

        await context.SaveChangesAsync();

        Console.WriteLine($"Готово. Добавени: Отдели={addedDiv}, Класове={addedClass}, Семейства={addedFamily}, Родове={addedGenus}, Видове={addedSpecies}");
        return 0;
    }
}

file class TaxonomicImportFile
{
    public List<SpeciesRecord>? Species { get; set; }
}

file class SpeciesRecord
{
    [System.Text.Json.Serialization.JsonPropertyName("division")]
    public string Division { get; set; } = "";

    [System.Text.Json.Serialization.JsonPropertyName("taxonomyClass")]
    public string TaxonomyClass { get; set; } = "";

    [System.Text.Json.Serialization.JsonPropertyName("family")]
    public string Family { get; set; } = "";

    [System.Text.Json.Serialization.JsonPropertyName("genus")]
    public string Genus { get; set; } = "";

    [System.Text.Json.Serialization.JsonPropertyName("species")]
    public string Species { get; set; } = "";
}
