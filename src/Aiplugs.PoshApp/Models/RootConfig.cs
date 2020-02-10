using System.Collections.Generic;
using System.Linq;

namespace Aiplugs.PoshApp.Models
{
    public class RootConfig
    {
        public static int FREE_PLAN_MAX_REPOSITORIES = 2;
        public static int FREE_PLAN_MAX_SCRIPTS = 10;
        public string ActivationCode { get; set; }
        public IEnumerable<Repository> Repositories { get; set; } = Enumerable.Empty<Repository>();
    }
}