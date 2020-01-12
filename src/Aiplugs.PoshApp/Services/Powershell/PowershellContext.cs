using Aiplugs.PoshApp.Services.Powersehll.Commands;
using System.Collections.Concurrent;

namespace Aiplugs.PoshApp.Services.Powersehll
{
    public class PowershellContext
    {
        private readonly ConcurrentQueue<InvokeCommand> _invokeQueue = new ConcurrentQueue<InvokeCommand>();
        private readonly ConcurrentDictionary<string, object> _cancelTable = new ConcurrentDictionary<string, object>();
        public readonly ConcurrentDictionary<string, PowershellIO> IO = new ConcurrentDictionary<string, PowershellIO>();

        public void Start(string connectionId)
        {
            IO.AddOrUpdate(connectionId, key => new PowershellIO(), (key, current) => new PowershellIO());
        }
        public void Invoke(InvokeCommand cmd)
        {
            _invokeQueue.Enqueue(cmd);
        }
        public bool TryDequeueCommand(out InvokeCommand cmd)
        {
            return _invokeQueue.TryDequeue(out cmd);
        }
        public bool IsCancel(string connectionId)
        {
            return !string.IsNullOrEmpty(connectionId) && _cancelTable.ContainsKey(connectionId);
        }
        public void Cancel(string connectionId)
        {
            _cancelTable.AddOrUpdate(connectionId, key => new object(), (key, current) => new object());
        }
        public void FinalizeCancel(string connectionId)
        {
            IO.TryRemove(connectionId, out var _);
            _cancelTable.TryRemove(connectionId, out var __);
        }
    }
}