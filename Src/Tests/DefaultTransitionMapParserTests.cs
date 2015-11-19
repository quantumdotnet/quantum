namespace Tests
{
    using System.Linq;
    using Microsoft.FSharp.Collections;
    using NUnit.Framework;
    using Quantum.BuilderFactory;

    [TestFixture]
    public class DefaultTransitionMapParserTests
    {
        [Test]
        public void Call_SingleLine_ShouldParseAllNodes()
        {
            FSharpList<Parsing.TransitionDef> result = Parse("NODE1 => NODE2");

            Assert.That(result.Length, Is.EqualTo(2));
            Assert.That(result.Head.Code, Is.EqualTo("NODE1"));
            Assert.That(result.Tail.Head.Code, Is.EqualTo("NODE2"));
        }

        [Test]
        public void Call_SingleLine_ShouldParseChildren()
        {
            FSharpList<Parsing.TransitionDef> result = Parse("NODE1 => NODE2");

            Assert.That(result.Length, Is.EqualTo(2));
            Assert.That(result.Head.ChildCodes.Head, Is.EqualTo("NODE2"));
            Assert.That(result.Tail.Head.ChildCodes.IsEmpty);
        } 

        [Test]
        public void Call_OptionalNode_ShouldAddFollowingItemsToChildren()
        {
            FSharpList<Parsing.TransitionDef> result = Parse("SELECT => FROM => WHERE? => ORDERBY");

            Parsing.TransitionDef from = result.First(x => x.Code == "FROM");

            Assert.That(from.ChildCodes.Length, Is.EqualTo(2));
            Assert.That(from.ChildCodes, Contains.Item("WHERE"));
            Assert.That(from.ChildCodes, Contains.Item("ORDERBY"));
        } 

        [Test]
        public void Call_Multiline_ShouldMergeChains()
        {
            FSharpList<Parsing.TransitionDef> result = Parse(
                "ROOT => SELECT => FROM => WHERE? => ORDERBY\r\n" +
                "                  FROM => INNERJOIN => ON => WHERE?");

            Parsing.TransitionDef from = result.First(x => x.Code == "FROM");

            Assert.That(from.ChildCodes.Length, Is.GreaterThanOrEqualTo(2));
            Assert.That(from.ChildCodes, Contains.Item("WHERE"));
            Assert.That(from.ChildCodes, Contains.Item("INNERJOIN"));
        } 

        [Test]
        public void Call_Multiline_ShouldMergeDeepChains()
        {
            FSharpList<Parsing.TransitionDef> result = Parse(
                "ROOT => SELECT => FROM => WHERE? => ORDERBY\r\n" +
                "                  FROM => INNERJOIN => ON => WHERE?");

            Parsing.TransitionDef on = result.First(x => x.Code == "ON");

            Assert.That(on.ChildCodes, Contains.Item("WHERE"));
            Assert.That(on.ChildCodes, Contains.Item("ORDERBY"));
        } 

        [Test]
        public void Call_Multiline_ShouldGiveNames()
        {
            FSharpList<Parsing.TransitionDef> result = Parse(
                "ROOT => SELECT => FROM => WHERE? => ORDERBY\r\n" +
                "                  FROM => INNERJOIN => ON => WHERE?");

            Parsing.TransitionDef join = result.First(x => x.Code == "INNERJOIN");

            Assert.That(join.Name, Is.EqualTo("FromInnerjoin"));
        } 

        [Test]
        public void Call_OptionalNode_ShouldGiveNames()
        {
            FSharpList<Parsing.TransitionDef> result = Parse("SELECT => FROM => WHERE? => ORDERBY");

            Parsing.TransitionDef select = result.First(x => x.Code == "SELECT");
            Parsing.TransitionDef from = result.First(x => x.Code == "FROM");
            Parsing.TransitionDef where = result.First(x => x.Code == "WHERE");
            Parsing.TransitionDef orderby = result.First(x => x.Code == "ORDERBY");

            Assert.That(select.Name, Is.EqualTo("Select"));
            Assert.That(from.Name, Is.EqualTo("SelectFrom"));
            Assert.That(where.Name, Is.EqualTo("SelectFromWhere"));
            Assert.That(orderby.Name, Is.EqualTo("SelectFromOrderby"));
        } 

        [Test]
        public void Call_Inheritance_ShouldInheritChildren()
        {
            FSharpList<Parsing.TransitionDef> result = Parse(
                "ROOT => SELECT => FROM => WHERE? => GROUPBY? => ORDERBY?\r\n" +
                "                  FROM => INNERJOIN => ON:FROM");

            Parsing.TransitionDef on = result.First(x => x.Code == "ON");

            Assert.That(on.ChildCodes.Length, Is.GreaterThan(0));
            Assert.That(on.ChildCodes, Contains.Item("WHERE"));
            Assert.That(on.ChildCodes, Contains.Item("INNERJOIN"));
            Assert.That(on.ChildCodes, Contains.Item("GROUPBY"));
            Assert.That(on.ChildCodes, Contains.Item("ORDERBY"));
        } 

        [Test]
        public void Call_Inheritance_ShouldGiveCorrectName()
        {
            FSharpList<Parsing.TransitionDef> result = Parse(
                "ROOT => SELECT => FROM => WHERE? => GROUPBY? => ORDERBY?\r\n" +
                "                  FROM => INNERJOIN => ON:FROM");

            Parsing.TransitionDef on = result.First(x => x.Code == "ON");

            Assert.That(on, Is.Not.Null);
        }

        private static FSharpList<Parsing.TransitionDef> Parse(string input)
        {
            return Parsing.parse.Invoke(input);
        }
    }
}