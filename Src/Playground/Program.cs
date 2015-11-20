namespace Playground
{
    using System;
    using Quantum;
    using Quantum.Common;
    using Quantum.Common.Definitions;

    public class People : TableDefinition
    {
        public People() : base("[dbo].[People]")
        {
            ID = new ColumnDefinition("Id");
            NAME = new ColumnDefinition("Name");
            AGE = new ColumnDefinition("Age");
        }

        /* Columns */
        public ColumnDefinition AGE { get; private set; }
        public ColumnDefinition NAME { get; private set; }
        public ColumnDefinition ID { get; private set; }
    }

    public static class Db
    {
        /* Tables */
        public static readonly People PEOPLE = new People();
    }

    class Program
    {
        private static void SET(params SqlValueExpression[] exprs) // just for testing
        {
        }

        static void Main(string[] args)
        {
            var @minId = "MinId";

            SQL.Value("hello");

            var query1 = SQL
                    .SELECT("Id")
                    .FROM("People")
                    .INNER_JOIN("Employee").ON("Id = PersonId")
                    .INNER_JOIN("City").ON("Id = CityId")
                    .WHERE(Db.PEOPLE.NAME >= "John" & Db.PEOPLE.ID > @minId)
                    .ORDERBY(new SqlExpression("Name"))
                    
                .BuildQuery();

            //var query2 = new RootNode().SELECT().FROM().WHERE().ORDERBY().BuildQuery();

            SqlPredicate p1 = SQL.Value("1") >= SQL.Value("2");
            SqlPredicate p2 = SQL.Value("1") >= SQL.Value("2");

            SqlPredicate p = p1 & p2 | p1;

            Console.WriteLine(p.QueryPartValue);

            SET(
                Db.PEOPLE.ID    <~ SQL.Value("Hello"),
                Db.PEOPLE.NAME  <~ SQL.Value("John Doe"),
                Db.PEOPLE.AGE   <~ (Db.PEOPLE.AGE + "2"));

            string query = SQL

                    .SELECT(
                        Db.PEOPLE.ID, 
                        Db.PEOPLE.NAME                 >~  SQL.Alias("MyName"),
                        Db.PEOPLE.AGE + Db.PEOPLE.NAME >~  SQL.Alias("AgeName"))
                    .FROM(Db.PEOPLE)

                .BuildQuery();

            Console.WriteLine(query);
            Console.WriteLine();
            Console.WriteLine(query1);
            Console.ReadLine();
        }
    }
}
