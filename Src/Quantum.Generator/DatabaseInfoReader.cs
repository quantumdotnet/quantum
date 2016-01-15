using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace Quantum.Generator
{
    public class DatabaseInfoReader : IDatabaseInfoReader
    {
        private readonly string _connectionString;

        public DatabaseInfoReader(string connectionString)
        {
            _connectionString = connectionString;
        }

        public IList<DatabaseTableInfo> GetSimpleTableStructure()
        {
            IList<DatabaseTableInfo> databaseTableInfos = new List<DatabaseTableInfo>();
            
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                DataTable schema = connection.GetSchema("Tables");

                foreach (DataRow row in schema.Rows)
                {
                    DatabaseTableInfo table = new DatabaseTableInfo { Name = row[2].ToString() };
                    string[] restrictionsColumns = new string[4];
                    restrictionsColumns[2] = table.Name;
                    DataTable schemaColumns = connection.GetSchema("Columns", restrictionsColumns);
                    IList<DatabaseFieldInfo> fileds = (from DataRow rowColumn in schemaColumns.Rows let columnName = rowColumn[3].ToString() let type = rowColumn[7].ToString() select new DatabaseFieldInfo() {Name = columnName, Type = type}).ToList();
                    table.Fields = fileds;

                    databaseTableInfos.Add(table);
                }

                foreach (var item in databaseTableInfos)
                {
                    Console.WriteLine(item.ClassName);

                    foreach (var pair in item.Fields)
                    {
                        Console.WriteLine(" - " + pair.AttributeName + "(" + pair.Type + ")");
                    }
                }                

                return databaseTableInfos;
            }
        }
    }
}
