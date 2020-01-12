using Aiplugs.PoshApp.Services.Git.Commands;
using System.Collections.Concurrent;


namespace Aiplugs.PoshApp.Services.Git
{
    public class GitContext
    {
        private readonly ConcurrentQueue<GitCommand> _invokeQueue = new ConcurrentQueue<GitCommand>();
        private readonly ConcurrentDictionary<string, object> _cancelTable = new ConcurrentDictionary<string, object>();
        public readonly ConcurrentDictionary<string, GitIO> IO = new ConcurrentDictionary<string, GitIO>();
        public void Invoke(GitCommand cmd)
        {
            _invokeQueue.Enqueue(cmd);
        }
        public bool TryDequeueCommand(out GitCommand cmd)
        {
            return _invokeQueue.TryDequeue(out cmd);
        }
        public void Start(string connectionId)
        {
            IO.AddOrUpdate(connectionId, key => new GitIO(), (key, current) => new GitIO());
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