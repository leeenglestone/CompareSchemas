using CompareSchemas.Domain.Interfaces;

namespace CompareSchemas.Domain.Concretions
{
    public class Server : IServer
    {
        public string Name { get; set; }
        public string Version { get; set; }
        public IDatabase Database { get; set; }
    }
}
