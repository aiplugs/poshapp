using System.ComponentModel.DataAnnotations;

namespace Aiplugs.PoshApp.ViewModels
{
    public class SingletonScriptViewModel
    {
        [Required]
        [StringLength(ViewModelConstants.ID_LENGTH)]
        public string Id { get; set; }
        public string DisplayName { get; set; }

        [StringLength(64)]
        public string Group { get; set; }
        
        public string CustomView { get; set; }

        public string[] Actions { get; set; }
    }
}