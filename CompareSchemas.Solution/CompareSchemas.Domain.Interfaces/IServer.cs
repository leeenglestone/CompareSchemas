namespace CompareSchemas.Domain.Interfaces
{
    public interface IServer
    {
        string Name { get; set; }
        string Version { get; set; }
        IDatabase Database { get; set; }
    }
}
