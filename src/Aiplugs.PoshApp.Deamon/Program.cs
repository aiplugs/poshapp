using Aiplugs.PoshApp.Deamon.PowerShell;
using StreamJsonRpc;
using System;
using System.Threading.Tasks;

namespace Aiplugs.PoshApp.Deamon
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await new PoshAppService().StartAsync();
        }
    }
}
