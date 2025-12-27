using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LeafMap_Insights.Models
{
    public class Tree
    {
        public int Id { get; set; }

        [Required]
        public int SpeciesId { get; set; }

        [Required]
        [Column(TypeName = "decimal(10, 8)")]
        public decimal Latitude { get; set; }

        [Required]
        [Column(TypeName = "decimal(11, 8)")]
        public decimal Longitude { get; set; }

        [Range(0, 10000)]
        public int? EstimatedAge { get; set; }

        [Range(0, 200)]
        [Column(TypeName = "decimal(5, 2)")]
        public decimal? Height { get; set; }

        [StringLength(50)]
        public string? Condition { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        public string? QRCodeValue { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation property
        [ForeignKey(nameof(SpeciesId))]
        public virtual Species Species { get; set; } = null!;
    }
}
