using System.ComponentModel.DataAnnotations;

namespace LeafMap_Insights.Models.ViewModels
{
    public class TreeViewModel
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Вид")]
        public int SpeciesId { get; set; }

        [Required]
        [Display(Name = "Географска ширина")]
        [Range(-90, 90)]
        public decimal Latitude { get; set; }

        [Required]
        [Display(Name = "Географска дължина")]
        [Range(-180, 180)]
        public decimal Longitude { get; set; }

        [Display(Name = "Приблизителна възраст (години)")]
        [Range(0, 10000)]
        public int? EstimatedAge { get; set; }

        [Display(Name = "Височина (метри)")]
        [Range(0, 200)]
        public decimal? Height { get; set; }

        [Display(Name = "Състояние")]
        [StringLength(50)]
        public string? Condition { get; set; }

        public List<Species>? AvailableSpecies { get; set; }
    }
}

