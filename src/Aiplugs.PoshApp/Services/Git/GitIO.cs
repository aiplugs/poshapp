using System.Collections.Concurrent;


namespace Aiplugs.PoshApp.Services.Git
{
    public class GitIO
    {
        public readonly ConcurrentQueue<(string username, string password)> CredentialQueue = new ConcurrentQueue<(string username, string password)>();
    }
}