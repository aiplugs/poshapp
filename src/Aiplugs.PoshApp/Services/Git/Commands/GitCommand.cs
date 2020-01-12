namespace Aiplugs.PoshApp.Services.Git.Commands
{
    public abstract class GitCommand 
    {
        public string ConnectionId { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
    }
}