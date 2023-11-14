namespace SistemaDSE.Models
{
    public class ErrorViewModel
    {
        /* Modelo para manejo de errores*/
        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}