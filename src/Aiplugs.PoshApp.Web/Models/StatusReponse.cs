using System.Collections;
using System.Collections.Generic;

namespace Aiplugs.PoshApp.Web.Models
{
    public class StatusReponse
    {
        public string File { get; set; }
        public IEnumerable<string> Labels { get; set; }
    }
}
