using CompareSchemas.Domain.Enums;

namespace CompareSchemas.Domain.Interfaces
{
    public interface IComparisonItem
    {
        string Name { get; set; }
        SchemaItemType ItemType { get; set; }
        string ItemSqlA { get; set; }
        string ItemSqlB { get; set; }
        bool ExistsInDatabaseA { get; set; }
        bool ExistsInDatabaseB { get; set; }
        bool IsDifferent { get; set; }
        bool ExistsInBothDatabases { get; }
    }
}
