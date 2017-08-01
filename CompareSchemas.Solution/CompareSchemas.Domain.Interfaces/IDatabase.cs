using System.Collections.Generic;

namespace CompareSchemas.Domain.Interfaces
{
    public interface IDatabase
    {
        string Name { get; set; }
        string ServerName { get; set; }
        IList<ISchemaItem> Tables { get; set; }
        IList<ISchemaItem> StoredProcedures { get; set; }
        IList<ISchemaItem> Views { get; set; }
        IList<ISchemaItem> Schemas { get; set; }
        IList<ISchemaItem> Users { get; set; }
        IList<ISchemaItem> UserDefinedFunctions { get; set; }
    }
}
