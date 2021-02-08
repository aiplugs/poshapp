using System;
using System.Globalization;
using System.Management.Automation.Host;

namespace Aiplugs.PoshApp.Pses
{
    public class PSESHost : PSHost
    {
        public override CultureInfo CurrentCulture => CultureInfo.InvariantCulture;

        public override CultureInfo CurrentUICulture => CultureInfo.InvariantCulture;

        public override Guid InstanceId { get; } = Guid.NewGuid();

        public override PSHostUserInterface UI { get; } = new PSESUserInterface();

        public override string Name => "poshapp pses";

        public override Version Version => new Version("1.0.0");

        public override void EnterNestedPrompt()
        {
        }

        public override void ExitNestedPrompt()
        {
        }

        public override void NotifyBeginApplication()
        {
        }

        public override void NotifyEndApplication()
        {
        }

        public override void SetShouldExit(int exitCode)
        {
        }
    }
}
