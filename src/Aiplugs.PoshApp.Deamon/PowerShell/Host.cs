using System;
using System.Globalization;
using System.Management.Automation.Host;

namespace Aiplugs.PoshApp.Deamon
{
    public class Host : PSHost
    {
        public Host(PSHostUserInterface ui)
        {
            _ui = ui;
        }
        public override CultureInfo CurrentCulture => CultureInfo.InvariantCulture;

        public override CultureInfo CurrentUICulture => CultureInfo.InvariantCulture;

        public override Guid InstanceId { get; } = Guid.NewGuid();

        private readonly PSHostUserInterface _ui;
        public override PSHostUserInterface UI => _ui;

        public override string Name => "poshapp";

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
