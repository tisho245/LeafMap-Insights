using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LeafMap_Insights.Models
{
    public class Genus
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Името е задължително")]
        [StringLength(100)]
        [Display(Name = "Име")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Латинското име е задължително")]
        [StringLength(200)]
        [Display(Name = "Латинско име")]
        public string LatinName { get; set; } = string.Empty;

        [StringLength(1000)]
        [Display(Name = "Описание")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Семейството е задължително")]
        [Display(Name = "Семейство")]
        public int FamilyId { get; set; }

        // Navigation properties
        [ForeignKey(nameof(FamilyId))]
        public virtual Family Family { get; set; } = null!;

        public virtual ICollection<Species> Species { get; set; } = new List<Species>();
    }
}
