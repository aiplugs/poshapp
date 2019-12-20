using System.ComponentModel.DataAnnotations;

namespace Aiplugs.PoshApp.ViewModels
{
    public class DetailScriptViewModel
    {
        [Required]
        [StringLength(ViewModelConstants.ID_LENGTH)]
        public string Id { get; set; }

        public string DisplayName { get; set; }

        public string[] Actions { get; set; }
    }
}