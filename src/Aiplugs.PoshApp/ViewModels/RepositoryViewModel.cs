using System.ComponentModel.DataAnnotations;

namespace Aiplugs.PoshApp.ViewModels
{
    public class RepositoryViewModel
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string Path { get; set; }

        public string Origin { get; set; }

        [Required]
        public string ConnectionId { get; set; }
    }
}