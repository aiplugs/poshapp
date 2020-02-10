using System.Collections.Generic;
using System.Linq;

namespace Aiplugs.PoshApp.Models
{
    public class Config
    {
        public IEnumerable<Script> Scripts { get; set; } = Enumerable.Empty<Script>();
    }
}