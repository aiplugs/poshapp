using System.ComponentModel.DataAnnotations;

namespace Aiplugs.PoshApp.ViewModels
{
    public class ActivationViewModel
    {
        [Required]
        public string ActivationCode { get; set; }
    }
}
