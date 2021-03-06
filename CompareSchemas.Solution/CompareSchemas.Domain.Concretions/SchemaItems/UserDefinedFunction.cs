﻿using CompareSchemas.Domain.Interfaces;
using CompareSchemas.Domain.Interfaces.SchemaItems;
using CompareSchemas.Domain.Enums;

namespace CompareSchemas.Domain.Concretions.SchemaItems
{
    public class UserDefinedFunction : ISchemaItem, IUserDefinedFunction
    {
        public string Name { get; set; }
        public SchemaItemType ItemType { get; set; }
        public string CreateSql { get; set; }

        public UserDefinedFunction(string name, string createSql)
        {
            Name = name;
            CreateSql = createSql;
        }
    }
}
