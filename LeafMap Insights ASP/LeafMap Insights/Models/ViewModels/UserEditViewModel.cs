using System.ComponentModel.DataAnnotations;

namespace LeafMap_Insights.Models.ViewModels
{
    public class UserEditViewModel
    {
        public string Id { get; set; } = string.Empty;

        [Required(ErrorMessage = "Имейл адресът е задължителен")]
        [EmailAddress(ErrorMessage = "Невалиден имейл адрес")]
        [Display(Name = "Имейл")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Потребителското име е задължително")]
        [Display(Name = "Потребителско име")]
        public string UserName { get; set; } = string.Empty;

        [Display(Name = "Имейлът е потвърден")]
        public bool EmailConfirmed { get; set; }

        [Display(Name = "Роли")]
        public List<string> AvailableRoles { get; set; } = new List<string>();

        public List<string>? SelectedRoles { get; set; } = new List<string>();
    }
}
