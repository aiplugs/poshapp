using System;
using System.Threading.Tasks;

namespace Aiplugs.PoshApp.Pses
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            var sendingStream = Console.OpenStandardOutput();
            var receivingStream = Console.OpenStandardInput();

            await new PSESService(sendingStream, receivingStream).StartAsync();
        }
    }
}
