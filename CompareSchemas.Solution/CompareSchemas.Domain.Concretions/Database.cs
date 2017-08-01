using CompareSchemas.Domain.Interfaces;
using System.Collections.Generic;

namespace CompareSchemas.Domain.Concretions
{
    public class Database : IDatabase
    {
        public string Name { get; set; }
        public string ServerName { get; set; }
        public IList<ISchemaItem> Tables { get; set; } = new List<ISchemaItem>();
        public IList<ISchemaItem> StoredProcedures { get; set; } = new List<ISchemaItem>();
        public IList<ISchemaItem> Views { get; set; } = new List<ISchemaItem>();
        public IList<ISchemaItem> Schemas { get; set; } = new List<ISchemaItem>();
        public IList<ISchemaItem> Users { get; set; } = new List<ISchemaItem>();
        public IList<ISchemaItem> UserDefinedFunctions { get; set; } = new List<ISchemaItem>();

        public Database(string name, string serverName)
        {
            Name = name;
            ServerName = serverName;
        }
    }
}
