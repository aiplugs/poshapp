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
            await new PoshAppService(sendingStream, receivingStream).StartAsync();
        }
    }
}
