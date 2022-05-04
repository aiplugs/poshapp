using Aiplugs.PoshApp.Pses;
using System;
using System.Threading.Tasks;

namespace Aiplugs.PoshApp.Deamon
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var sendingStream = Console.OpenStandardOutput();
            var receivingStream = Console.OpenStandardInput();

            if (args.Length > 0 && args[0] == "pses")
            {
                await new PSESService(sendingStream, receivingStream).StartAsync();
            }
            else
            {
                await new PoshAppService(sendingStream, receivingStream).StartAsync();
            }
        }
    }
}
