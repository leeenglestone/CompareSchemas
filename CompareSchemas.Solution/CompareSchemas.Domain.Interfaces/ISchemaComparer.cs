namespace CompareSchemas.Domain.Interfaces
{
    public interface ISchemaComparer
    {
        IComparisonResult CompareDatabases(string serverNameA, string serverNameB, IDatabase databaseA, IDatabase databaseB);
        IDatabase GetDatabase(string serverName, string databaseName, string username, string password);
        void EmailResult(IComparisonResult comparisonResult);
    }
}
