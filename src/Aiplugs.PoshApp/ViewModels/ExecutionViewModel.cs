using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Aiplugs.PoshApp.ViewModels
{
    public class ExecutionViewModel
    {
        [Required]
        public string Id { get; set;}
        public string PipelineParameter { get; set; }
        public IDictionary<string, string> Parameters { get; set; }
    }
}