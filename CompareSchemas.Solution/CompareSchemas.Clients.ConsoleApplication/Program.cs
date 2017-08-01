using System;
using CompareSchemas.Domain.Concretions;
using CompareSchemas.Domain.Interfaces;
using System.Threading.Tasks;

namespace CompareSchemas.Clients.ConsoleApplication
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            const string missingParametersMessage = "Missing required parameters [databaseName] [serverNameA] [serverNameB]";

            if (args == null)
            {
                Console.WriteLine(missingParametersMessage);
                Console.ReadLine();
                return;
            }

            if (args.Length < 3)
            {
                Console.WriteLine(missingParametersMessage);
                Console.ReadLine();
                return;
            }

            var databaseName = args[0];
            var serverNameA = args[1];
            var serverNameB = args[2];

            IDatabase databaseA = null;
            IDatabase databaseB = null;

            var schemaComparer = new SchemaComparer();

            Task task1 = Task.Factory.StartNew(() => databaseA = schemaComparer.GetDatabase(serverNameA, databaseName));
            Task task2 = Task.Factory.StartNew(() => databaseB = schemaComparer.GetDatabase(serverNameB, databaseName));

            Task.WaitAll(task1, task2);

            var comparisonResult = schemaComparer.CompareDatabases(serverNameA, serverNameB, databaseA, databaseB);

            schemaComparer.EmailResult(comparisonResult);
        }
    }
}
