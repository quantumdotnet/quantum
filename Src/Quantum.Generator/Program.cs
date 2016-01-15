using System;
using System.Collections.Generic;

namespace Quantum.Generator
{
    class Program
    {                
        static void Main()
        {
            Console.WriteLine("Please enter connection string to database");
            var connectionString = Console.ReadLine();

            Console.WriteLine("Please enter namespace");
            var @namespace = Console.ReadLine();

            Console.WriteLine("Please enter path for generated files");
            var path = Console.ReadLine();

            IDatabaseInfoReader reader = new DatabaseInfoReader(connectionString);            
            IList<DatabaseTableInfo> tables = reader.GetSimpleTableStructure();

            Console.WriteLine("Start generation...");

            ICodeGenerator generator = new CodeGenerator(@namespace, path, "Model");
            generator.GenerateClasses(tables);
            
            Console.WriteLine("Generation completed!");
            Console.ReadLine();
        }
    }
}
