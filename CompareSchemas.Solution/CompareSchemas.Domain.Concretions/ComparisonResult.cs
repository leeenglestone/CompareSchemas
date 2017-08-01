using CompareSchemas.Domain.Interfaces;
using System.Collections.Generic;

namespace CompareSchemas.Domain.Concretions
{
    public class ComparisonResult : IComparisonResult
    {
        public IList<IComparisonItem> ComparisonItems { get; set; }
        public string DatabaseName { get; set; }
        public string ServerNameA { get; set; }
        public string ServerNameB { get; set; }

        public ComparisonResult(string databaseName, string serverNameA, string serverNameB, IList<IComparisonItem> comparisonItems)
        {
            DatabaseName = databaseName;
            ServerNameA = serverNameA;
            ServerNameB = serverNameB;
            ComparisonItems = comparisonItems;
        }
    }
}
