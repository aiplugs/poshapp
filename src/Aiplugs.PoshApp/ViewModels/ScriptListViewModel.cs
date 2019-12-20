using System.Collections.Generic;
using Aiplugs.PoshApp.Models;

namespace Aiplugs.PoshApp.ViewModels
{
    public class ScriptListViewModel
    {
        public IEnumerable<ScriptListItemViewModel> Scripts { get; set; }
    }
    public class ScriptListItemViewModel
    {
        public string Id { get; set; }
        public ScriptType Type { get; set; }
    }
}