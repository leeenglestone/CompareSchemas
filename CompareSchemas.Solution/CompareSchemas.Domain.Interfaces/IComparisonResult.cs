using System.Collections.Generic;

namespace CompareSchemas.Domain.Interfaces
{
    public interface IComparisonResult
    {
        string DatabaseName { get; set; }
        string ServerNameA { get; set; }
        string ServerNameB { get; set; }
        IList<IComparisonItem> ComparisonItems { get; set; }
    }
}
