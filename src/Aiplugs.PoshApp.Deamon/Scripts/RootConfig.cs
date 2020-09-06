using System.Collections.Generic;
using System.Linq;

namespace Aiplugs.PoshApp.Models
{
    public class RootConfig
    {
        public string ActivationCode { get; set; }
        public IEnumerable<Repository> Repositories { get; set; } = Enumerable.Empty<Repository>();
    }
}