using System.ComponentModel.DataAnnotations;

namespace LeafMap_Insights.Models
{
    public class Family
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Името е задължително")]
        [StringLength(100)]
        [Display(Name = "Име")]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000)]
        [Display(Name = "Описание")]
        public string? Description { get; set; }

        // Navigation property
        public virtual ICollection<Genus> Genera { get; set; } = new List<Genus>();
    }
}
