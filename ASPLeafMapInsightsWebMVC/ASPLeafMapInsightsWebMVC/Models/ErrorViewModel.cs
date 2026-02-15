namespace ASPLeafMapInsightsWebMVC.Models
{
    /// <summary>Модел за страницата Home/Error – показва RequestId при необработено изключение.</summary>
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
