using CompareSchemas.Domain.Interfaces;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Mail;
using CompareSchemas.Domain.Enums;
using DiffMatchPatch;

namespace CompareSchemas.Domain.Concretions
{
    public class SchemaComparer : ISchemaComparer
    {
        public IComparisonResult CompareDatabases(string serverNameA, string serverNameB, IDatabase databaseA, IDatabase databaseB)
        {
            var comparisonItems = new List<IComparisonItem>();

            // Loop through first db, add to central list
            comparisonItems.AddRange(from SchemaItems.Table table in databaseA.Tables select new ComparisonItem(table.Name, SchemaItemType.Table, true, false, table.CreateSql));
            comparisonItems.AddRange(from SchemaItems.StoredProcedure storedProcedure in databaseA.StoredProcedures select new ComparisonItem(storedProcedure.Name, SchemaItemType.StoredProcedure, true, false, storedProcedure.CreateSql));
            comparisonItems.AddRange(from SchemaItems.View view in databaseA.Views select new ComparisonItem(view.Name, SchemaItemType.View, true, false, view.CreateSql));
            comparisonItems.AddRange(from SchemaItems.Schema schema in databaseA.Schemas select new ComparisonItem(schema.Name, SchemaItemType.Schema, true, false, schema.CreateSql));
            comparisonItems.AddRange(from SchemaItems.UserDefinedFunction userDefinedFunction in databaseA.UserDefinedFunctions select new ComparisonItem(userDefinedFunction.Name, SchemaItemType.UserDefinedFunction, true, false, userDefinedFunction.CreateSql));
            comparisonItems.AddRange(from SchemaItems.User user in databaseA.Users select new ComparisonItem(user.Name, SchemaItemType.User, true, false, user.CreateSql));

            // Loop through second db, add to central list if missing
            comparisonItems.AddRange(AddItemToDatabaseBIfMissingFromDatabaseA(databaseB.Tables, comparisonItems.Where(x => x.ItemType == SchemaItemType.Table).ToList()));
            comparisonItems.AddRange(AddItemToDatabaseBIfMissingFromDatabaseA(databaseB.StoredProcedures, comparisonItems.Where(x => x.ItemType == SchemaItemType.StoredProcedure).ToList()));
            comparisonItems.AddRange(AddItemToDatabaseBIfMissingFromDatabaseA(databaseB.Views, comparisonItems.Where(x => x.ItemType == SchemaItemType.View).ToList()));
            comparisonItems.AddRange(AddItemToDatabaseBIfMissingFromDatabaseA(databaseB.UserDefinedFunctions, comparisonItems.Where(x => x.ItemType == SchemaItemType.UserDefinedFunction).ToList()));
            comparisonItems.AddRange(AddItemToDatabaseBIfMissingFromDatabaseA(databaseB.Users, comparisonItems.Where(x => x.ItemType == SchemaItemType.User).ToList()));
            comparisonItems.AddRange(AddItemToDatabaseBIfMissingFromDatabaseA(databaseB.Schemas, comparisonItems.Where(x => x.ItemType == SchemaItemType.Schema).ToList()));

            // Check if different, if in both databases
            foreach (var item in comparisonItems)
            {
                if (!item.ExistsInBothDatabases)
                    continue;

                item.IsDifferent = !item.ItemSqlA.Trim().Equals(item.ItemSqlB.Trim());
            }

            return new ComparisonResult(databaseA.Name, serverNameA, serverNameB, comparisonItems);
        }

        private IEnumerable<IComparisonItem> AddItemToDatabaseBIfMissingFromDatabaseA(IEnumerable<ISchemaItem> schemaItems, List<IComparisonItem> comparisonItems)
        {
            foreach (var schemaItem in schemaItems)
            {
                if (!comparisonItems.Any(x => x.Name == schemaItem.Name && x.ItemType == schemaItem.ItemType))
                {
                    comparisonItems.Add(new ComparisonItem(schemaItem.Name, schemaItem.ItemType, false, true, string.Empty, schemaItem.CreateSql));
                }
                else
                {
                    var comparisonItem = comparisonItems.FirstOrDefault(x => x.Name == schemaItem.Name && x.ItemType == schemaItem.ItemType);
                    if (comparisonItem != null)
                    {
                        comparisonItem.ExistsInDatabaseB = true;
                        comparisonItem.ItemSqlB = schemaItem.CreateSql;
                    }
                }
            }

            return comparisonItems;
        }

        public IDatabase GetDatabase(string serverName, string databaseName)
        {
            IDatabase database;

            try
            {
                var serverConnection = new ServerConnection(serverName);
                serverConnection.LoginSecure = true;
                database = GetDatabase(serverConnection, databaseName);
            }
            catch(Exception ex)
            {
                throw new Exception($"Could not connect to {databaseName} on {serverName}", ex);
            }

            return database;
        }

        public IDatabase GetDatabase(string serverName, string databaseName, string username, string password)
        {
            var serverConnection = new ServerConnection(serverName)
            {
                ConnectAsUser = false,
                ConnectAsUserPassword = password,
                ConnectAsUserName = username
            };

            return GetDatabase(serverConnection, databaseName);
        }

        private IDatabase GetDatabase(ServerConnection serverConnection, string databaseName)
        {
            var myServer = new Microsoft.SqlServer.Management.Smo.Server(serverConnection);
            var myDatabase = myServer.Databases[databaseName];

            var database = new Database(myDatabase.Name, myServer.InstanceName);
            StringBuilder createSql;

            foreach (Table myTable in myDatabase.Tables)
            {
                Console.WriteLine($"{serverConnection.ServerInstance} {databaseName} Table {myTable.Name}");

                createSql = new StringBuilder();

                var tableScripts = myTable.Script();
                foreach (var script in tableScripts)
                    createSql.AppendLine(script);

                var table = new SchemaItems.Table(myTable.Name, createSql.ToString());
                database.Tables.Add(table);
            }

            foreach (StoredProcedure myStoredProcedure in myDatabase.StoredProcedures)
            {
                Console.WriteLine($"{serverConnection.ServerInstance} {databaseName} StoredProcedure {myStoredProcedure.Name}");

                if (myStoredProcedure.IsSystemObject)
                    continue;

                createSql = new StringBuilder();

                var tableScripts = myStoredProcedure.Script();
                foreach (var script in tableScripts)
                    createSql.AppendLine(script);

                var storedProcedure = new SchemaItems.StoredProcedure(myStoredProcedure.Name, createSql.ToString());
                database.StoredProcedures.Add(storedProcedure);
            }

            foreach (View myView in myDatabase.Views)
            {
                Console.WriteLine($"{serverConnection.ServerInstance} {databaseName} View {myView.Name}");

                if (myView.IsSystemObject)
                    continue;

                createSql = new StringBuilder();

                var tableScripts = myView.Script();
                foreach (var script in tableScripts)
                    createSql.AppendLine(script);

                var view = new SchemaItems.View(myView.Name, createSql.ToString());
                database.Views.Add(view);
            }

            foreach (User myUser in myDatabase.Users)
            {
                Console.WriteLine($"{serverConnection.ServerInstance} {databaseName} User {myUser.Name}");

                if (myUser.IsSystemObject)
                    continue;

                createSql = new StringBuilder();
                var tableScripts = myUser.Script();
                foreach (var script in tableScripts)
                    createSql.AppendLine(script);

                var user = new SchemaItems.User(myUser.Name, createSql.ToString());
                database.Users.Add(user);
            }

            foreach (UserDefinedFunction myUserDefinedFunction in myDatabase.UserDefinedFunctions)
            {
                Console.WriteLine($"{serverConnection.ServerInstance} {databaseName} UserDefinedFunction {myUserDefinedFunction.Name}");

                if (myUserDefinedFunction.IsSystemObject)
                    continue;

                createSql = new StringBuilder();
                var tableScripts = myUserDefinedFunction.Script();
                foreach (var script in tableScripts)
                    createSql.AppendLine(script);

                var userDefinedFunction = new SchemaItems.UserDefinedFunction(myUserDefinedFunction.Name, createSql.ToString());
                database.UserDefinedFunctions.Add(userDefinedFunction);
            }

            foreach (Schema mySchema in myDatabase.Schemas)
            {
                Console.WriteLine($"{serverConnection.ServerInstance} {databaseName} Schema {mySchema.Name}");

                if (mySchema.IsSystemObject)
                    continue;

                createSql = new StringBuilder();
                var tableScripts = mySchema.Script();
                foreach (var script in tableScripts)
                    createSql.AppendLine(script);

                var schema = new SchemaItems.Schema(mySchema.Name, createSql.ToString());
                database.Schemas.Add(schema);
            }

            return database;
        }

        public void EmailResult(IComparisonResult comparisonResult)
        {
            try
            {
                var emailBody = new StringBuilder();
                emailBody.AppendLine($"<p>The following schema differences were found in database <strong>{comparisonResult.DatabaseName}</strong> on <strong>{comparisonResult.ServerNameA}</strong> and <strong>{comparisonResult.ServerNameB}</strong></p>");

                var tableDifferences = comparisonResult.ComparisonItems.Count(x => x.ItemType == SchemaItemType.Table && x.IsDifferent);
                var storedProcedureDifferences = comparisonResult.ComparisonItems.Count(x => x.ItemType == SchemaItemType.StoredProcedure && x.IsDifferent);
                var viewDifferences = comparisonResult.ComparisonItems.Count(x => x.ItemType == SchemaItemType.View && x.IsDifferent);
                var userDefinedFunctionsDifferences = comparisonResult.ComparisonItems.Count(x => x.ItemType == SchemaItemType.UserDefinedFunction && x.IsDifferent);
                var userDifferences = comparisonResult.ComparisonItems.Count(x => x.ItemType == SchemaItemType.User && x.IsDifferent);
                var schemaDifferences = comparisonResult.ComparisonItems.Count(x => x.ItemType == SchemaItemType.Schema && x.IsDifferent);

                emailBody.AppendLine($@"<ul>
                <li><a href=""#Tables"">Tables ({tableDifferences})</a></li>
                <li><a href=""#StoredProcedures"">StoredProcedures ({storedProcedureDifferences})</a></li>
                <li><a href=""#Views"">Views ({viewDifferences})</a></li>
                <li><a href=""#UserDefinedFunctions"">UserDefinedFunctions ({userDefinedFunctionsDifferences})</a></li>
                <li><a href=""#Users"">Users ({userDifferences})</a></li>
                <li><a href=""#Schemas"">Schemas ({schemaDifferences})</a></li>
            </ul>");

                emailBody.AppendLine(AppendSchemaBody("Tables", SchemaItemType.Table, comparisonResult));
                emailBody.AppendLine(AppendSchemaBody("StoredProcedures", SchemaItemType.StoredProcedure, comparisonResult));
                emailBody.AppendLine(AppendSchemaBody("Views", SchemaItemType.View, comparisonResult));
                emailBody.AppendLine(AppendSchemaBody("UserDefinedFunctions", SchemaItemType.UserDefinedFunction, comparisonResult));
                emailBody.AppendLine(AppendSchemaBody("Users", SchemaItemType.User, comparisonResult));
                emailBody.AppendLine(AppendSchemaBody("Schemas", SchemaItemType.Schema, comparisonResult));

                using (var client = new SmtpClient(ConfigurationManager.AppSettings["Email.SmtpHost"]))
                {
                    var mailMessage = new MailMessage(
                        ConfigurationManager.AppSettings["Email.From"],
                        ConfigurationManager.AppSettings["Email.To"],
                        $"Schema Drift Report for {comparisonResult.DatabaseName} on {comparisonResult.ServerNameA} and {comparisonResult.ServerNameB}",
                        emailBody.ToString())
                    { IsBodyHtml = true };
                    client.Send(mailMessage);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"There was a problem sending the schema compare report email.", ex);
            }
        }

        private string AppendSchemaBody(string headingName, SchemaItemType itemType, IComparisonResult comparisonResult)
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine($"<h2>{headingName} with differences<a name=\"{headingName}\"></a></h2>");

            if (comparisonResult.ComparisonItems.Any(x => x.ItemType == itemType && x.IsDifferent))
            {
                foreach (var item in comparisonResult.ComparisonItems.Where(x => x.ItemType == itemType && x.IsDifferent))
                {
                    stringBuilder.AppendLine($"<h3>{item.Name}</h3>");

                    var itemSqlA = item.ItemSqlA.Replace("\r\n", "<br/>");
                    var itemSqlB = item.ItemSqlB.Replace("\r\n", "<br/>");

                    var diffMatchPatch = new diff_match_patch();
                    var diffs = diffMatchPatch.diff_main(item.ItemSqlA, item.ItemSqlB);
                    var diffHtml = diffMatchPatch.diff_prettyHtml(diffs);

                    stringBuilder.AppendLine(
                        $@"<table border=""1"" width=""100%""> 
                            <tr>
                                <th width=""33%"">{comparisonResult.ServerNameA}</th>
                                <th width=""33%"">{comparisonResult.ServerNameB}</th>
                                <th width=""33%"">Diff</th>
                            </tr>
                            <tr>
                                <td valign=""top"">{itemSqlA}</td>
                                <td valign=""top"">{itemSqlB}</td>
                                <td valign=""top"">{diffHtml}</td>
                            </tr>
                        </table>");
                }
            }
            else
            {
                stringBuilder.AppendLine("<p>None</p>");
            }

            return stringBuilder.ToString();
        }
    }
}
