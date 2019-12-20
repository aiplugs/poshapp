using System;
using System.Globalization;
using System.Management.Automation.Host;

namespace Aiplugs.PoshApp.Services
{
    public class PowershellHost : PSHost
    {
        public PowershellHost(PowershellUI ui)
        {
            _ui = ui;
            _instanceId = Guid.NewGuid();
        }
        public override CultureInfo CurrentCulture => CultureInfo.InvariantCulture;

        public override CultureInfo CurrentUICulture => CultureInfo.InvariantCulture;

        private readonly Guid _instanceId;
        public override Guid InstanceId => _instanceId;

        public override string Name => "poshapp";

        private readonly PSHostUserInterface _ui;
        public override PSHostUserInterface UI => _ui;

        public override Version Version => new Version(1, 0, 0);

        public event Action OnEnterNestedPrompt;
        public override void EnterNestedPrompt()
        {
            OnEnterNestedPrompt?.Invoke();
        }

        public event Action OnExitNestedPrompt;
        public override void ExitNestedPrompt()
        {
            OnExitNestedPrompt?.Invoke();
        }

        public event Action OnNotifyBeginApplication;
        public override void NotifyBeginApplication()
        {
            OnNotifyBeginApplication?.Invoke();
        }

        public event Action OnNotifyEndApplication;
        public override void NotifyEndApplication()
        {
            OnNotifyEndApplication?.Invoke();
        }

        public event Action OnSetShouldExit;
        public override void SetShouldExit(int exitCode)
        {
            OnSetShouldExit?.Invoke();
        }
    }
}