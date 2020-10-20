using System;
using System.Collections.Concurrent;
using System.Net;

namespace Aiplugs.PoshApp.Web.Models
{
    public class MemoryCredentialManager : ICredentialManager
    {
        private ConcurrentDictionary<string, NetworkCredential> _registry = new ConcurrentDictionary<string, NetworkCredential>();
        public NetworkCredential GetCredentials(string target)
        {
            return _registry.TryGetValue(target, out var credentials) ? credentials : null;
        }

        public void RemoveCredentials(string target)
        {
            _registry.TryRemove(target, out var _);
        }

        public void SaveCredentials(string target, NetworkCredential credentials)
        {
            _registry.TryAdd(target, credentials);
        }
    }
}
