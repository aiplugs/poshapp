namespace Aiplugs.PoshApp.Services.Powersehll.Commands
{
    public abstract class InvokeCommand 
    {
        public string ConnectionId { get; set; }
        public string ScriptId { get; set; }
    }
}