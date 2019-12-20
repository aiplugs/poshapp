using System.ComponentModel.DataAnnotations;

namespace Aiplugs.PoshApp.ViewModels
{
    public class ListScriptViewModel
    {
        [Required]
        [StringLength(ViewModelConstants.ID_LENGTH)]
        public string Id { get; set; }
        public string DisplayName { get; set; }

        [StringLength(64)]
        public string Group { get; set; }

        [StringLength(ViewModelConstants.ID_LENGTH)]
        public string Detail { get; set; }

        public string[] Actions { get; set; }
    }
}