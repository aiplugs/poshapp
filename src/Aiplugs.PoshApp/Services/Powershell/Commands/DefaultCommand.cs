using System.Collections.Generic;

namespace Aiplugs.PoshApp.Services.Powersehll.Commands
{
    public class DefaultCommand : InvokeCommand
    {
        public Dictionary<string, object> Parameters { get; set; }
    }
}