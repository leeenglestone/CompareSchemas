using CompareSchemas.Domain.Interfaces;
using CompareSchemas.Domain.Enums;
using CompareSchemas.Domain.Interfaces.SchemaItems;

namespace CompareSchemas.Domain.Concretions.SchemaItems
{
    public class Table : ISchemaItem, ITable
    {
        public string Name { get; set; }
        public SchemaItemType ItemType { get; set; }
        public string CreateSql { get; set; }

        public Table(string name, string createSql)
        {
            Name = name;
            CreateSql = createSql;
        }
    }
}
