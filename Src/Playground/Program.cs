namespace Playground
{
    using System;
    using Quantum;
    using Quantum.Common;
    using Quantum.Common.Definitions;
    using Quantum.QueryBuilder.MsSql;

    class People : TableDefinition
    {
        public People() : base("[dbo].[People]")
        {
        }

        public ColumnDefinition ID = new ColumnDefinition("Id");
        public ColumnDefinition NAME = new ColumnDefinition("Name");
        public ColumnDefinition _age = new ColumnDefinition("Age");

        public ColumnDefinition AGE
        {
            get { return _age; }
            set { }
        }
    }

    class Program
    {
        static readonly People PEOPLE = new People();

        private static void SET(params SqlValueExpression[] exprs)
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
                    .WHERE(PEOPLE.NAME >= "John" & PEOPLE.ID > @minId)
                    .ORDERBY(new SqlExpression("Name"))
                    
                .BuildQuery();

            //var query2 = new RootNode().SELECT().FROM().WHERE().ORDERBY().BuildQuery();

            SqlPredicate p1 = SQL.Value("1") >= SQL.Value("2");
            SqlPredicate p2 = SQL.Value("1") >= SQL.Value("2");

            SqlPredicate p = p1 & p2 | p1;

            Console.WriteLine(p.QueryPartValue);

            SET(
                PEOPLE.ID    <~ SQL.Value("Hello"),
                PEOPLE.NAME  <~ SQL.Value("John Doe"),
                PEOPLE.AGE   <~ (PEOPLE.AGE + "2"));

            string query = SQL

                    .SELECT(
                        PEOPLE.ID, 
                        PEOPLE.NAME                 >~  SQL.Alias("MyName"),
                        PEOPLE.AGE + PEOPLE.NAME    >~  SQL.Alias("AgeName"))
                    .FROM(PEOPLE)

                .BuildQuery();

            Console.WriteLine(query);
            Console.WriteLine();
            Console.WriteLine(query1);
            Console.ReadLine();
        }
    }
}
