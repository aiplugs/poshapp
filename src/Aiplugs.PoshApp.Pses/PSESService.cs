using Microsoft.Extensions.Logging;
using Microsoft.PowerShell.EditorServices.Hosting;
using System;
using System.IO;
using System.Management.Automation;
using System.Reflection;
using System.Threading.Tasks;

namespace Aiplugs.PoshApp.Pses
{
    public class PSESService
    {
        private readonly Stream _sendingStream;
        private readonly Stream _receivingStream;
        public PSESService(Stream sendingStream, Stream receivingStream)
        {
            _sendingStream = sendingStream;
            _receivingStream = receivingStream;
        }

        public async Task StartAsync()
        {
            var modulesPath = Path.Combine(Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName, "Modules");
            var psModulePath = Environment.GetEnvironmentVariable("PSModulePath").TrimEnd(Path.PathSeparator);
            psModulePath = $"{psModulePath}{Path.PathSeparator}{modulesPath}";
            Environment.SetEnvironmentVariable("PSModulePath", psModulePath);
            Environment.SetEnvironmentVariable("PSExecutionPolicyPreference", "Unrestricted");

            var loggerFactory = new LoggerFactory();

            var profilePaths = new ProfilePathInfo(
                currentUserAllHosts: GetFullProfileFileName(null, true, false),
                currentUserCurrentHost: GetFullProfileFileName(null, true, false),
                allUsersAllHosts: GetFullProfileFileName(null, false, false),
                allUsersCurrentHost: GetFullProfileFileName(null, false, false));
            var hostStartupInfo = new HostStartupInfo(
                name: "POSH App",
                profileId: "Aiplugs.PoshApp.Pses",
                version: new Version(0, 0, 0),
                psHost: new PSESHost(),
                profilePaths,
                featureFlags: new string[0],
                additionalModules: new string[0],
                languageMode: PSLanguageMode.FullLanguage,
                logPath: Path.GetTempFileName(),
                logLevel: (int)LogLevel.Error,
                consoleReplEnabled: false,
                usesLegacyReadLine: false);

            var lspServerType = Assembly.GetAssembly(typeof(HostStartupInfo)).GetType("Microsoft.PowerShell.EditorServices.Server.PsesLanguageServer");
            var lspServer = Activator.CreateInstance(lspServerType, new object[] { loggerFactory, _receivingStream, _sendingStream, hostStartupInfo });

            await (Task)lspServerType.GetMethod("StartAsync").Invoke(lspServer, null);
            await (Task)lspServerType.GetMethod("WaitForShutdown").Invoke(lspServer, null);
        }

        #region utilities
#if UNIX
        internal static readonly string ConfigDirectory = Platform.SelectProductNameForDirectory(Platform.XDG_Type.CONFIG);
#else
        internal static readonly string ConfigDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + @"\PowerShell";
#endif
        internal static string GetFullProfileFileName(string shellId, bool forCurrentUser, bool useTestProfile)
        {
            string basePath = null;

            if (forCurrentUser)
            {
                basePath = ConfigDirectory;
            }
            else
            {
                basePath = GetAllUsersFolderPath(shellId);
                if (string.IsNullOrEmpty(basePath))
                {
                    return string.Empty;
                }
            }

            string profileName = useTestProfile ? "profile_test.ps1" : "profile.ps1";

            if (!string.IsNullOrEmpty(shellId))
            {
                profileName = shellId + "_" + profileName;
            }

            string fullPath = basePath = Path.Combine(basePath, profileName);

            return fullPath;
        }

        private static string GetAllUsersFolderPath(string shellId)
        {
            string folderPath = string.Empty;
            try
            {
                folderPath = GetApplicationBase(shellId);
            }
            catch (System.Security.SecurityException)
            {
            }

            return folderPath;
        }
        internal static string GetApplicationBase(string shellId)
        {
            // Use the location of SMA.dll as the application base.
            Assembly assembly = typeof(PSObject).Assembly;
            return Path.GetDirectoryName(assembly.Location);
        }
        #endregion
    }
}
