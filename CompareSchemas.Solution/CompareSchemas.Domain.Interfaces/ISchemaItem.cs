
using CompareSchemas.Domain.Enums;

namespace CompareSchemas.Domain.Interfaces
{
    public interface ISchemaItem
    {
        string Name { get; set; }
        SchemaItemType ItemType { get; set; }
        string CreateSql { get; set; }
    }
}
