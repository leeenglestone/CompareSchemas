using CompareSchemas.Domain.Interfaces;
using CompareSchemas.Domain.Enums;

namespace CompareSchemas.Domain.Concretions
{
    public class ComparisonItem : IComparisonItem
    {
        public ComparisonItem(string name, SchemaItemType itemType, bool existsInDatabaseA, bool existsInDatabaseB, string itemSqlA = "", string itemSqlB = "")
        {
            Name = name;
            ItemType = itemType;
            ExistsInDatabaseA = existsInDatabaseA;
            ExistsInDatabaseB = existsInDatabaseB;
            ItemSqlA = itemSqlA;
            ItemSqlB = itemSqlB;
        }

        public string Name { get; set; }
        public SchemaItemType ItemType { get; set; }
        public string ItemSqlA { get; set; }
        public string ItemSqlB { get; set; }
        public bool ExistsInDatabaseA { get; set; }
        public bool ExistsInDatabaseB { get; set; }
        public bool IsDifferent { get; set; }
        public bool ExistsInBothDatabases => ExistsInDatabaseA && ExistsInDatabaseB;
    }
}
