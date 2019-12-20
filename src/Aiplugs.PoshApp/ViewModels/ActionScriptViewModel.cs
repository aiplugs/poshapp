using System.ComponentModel.DataAnnotations;

namespace Aiplugs.PoshApp.ViewModels
{
    public class ActionScriptViewModel
    {
        [Required]
        [StringLength(ViewModelConstants.ID_LENGTH)]
        public string Id { get; set; }
        public string DisplayName { get; set; }
    }
}