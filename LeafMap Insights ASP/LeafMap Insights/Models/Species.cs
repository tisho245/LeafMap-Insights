using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LeafMap_Insights.Models
{
    public class Species
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

        [StringLength(2000)]
        [Display(Name = "Описание")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Родът е задължителен")]
        [Display(Name = "Род")]
        public int GenusId { get; set; }

        // Navigation properties
        [ForeignKey(nameof(GenusId))]
        public virtual Genus Genus { get; set; } = null!;

        public virtual ICollection<Tree> Trees { get; set; } = new List<Tree>();
    }
}
