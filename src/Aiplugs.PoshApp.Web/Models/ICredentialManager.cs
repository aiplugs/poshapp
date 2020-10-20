using System.Net;

namespace Aiplugs.PoshApp.Web.Models
{
    public interface ICredentialManager
    {
        NetworkCredential GetCredentials(string target);

        void RemoveCredentials(string target);

        void SaveCredentials(string target, NetworkCredential credentials);
    }
}
