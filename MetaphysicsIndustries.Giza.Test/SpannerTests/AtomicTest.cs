using System;
using NUnit.Framework;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace MetaphysicsIndustries.Giza.Test.SpannerTests
{
    [TestFixture]
    public class AtomicTest
    {
        [Test]
        public void TestAtomic1()
        {
            // setup
            var sequence =
                new DefinitionExpression(
                    name: "sequence",
                    items: new [] {
                        new DefRefSubExpression("letters", isRepeatable: true)
                    }
                );
            var letters =
                new DefinitionExpression(
                    name: "letters",
                    directives: new [] { DefinitionDirective.Atomic },
                    items: new [] {
                        new CharClassSubExpression(
                            charClass: CharClass.FromUndelimitedCharClassText("\\l"),
                            isRepeatable: true
                        )
                    }
                );
            var defs = (new DefinitionBuilder()).BuildDefinitions(new [] { letters, sequence });
            var grammar = new Grammar(defs);
            var sequenceDef = grammar.FindDefinitionByName("sequence");
            var lettersDef = grammar.FindDefinitionByName("letters");
            var spanner = new Spanner(sequenceDef);
            var input = "abc";
            var errors = new List<Error>();

            // action
            var spans = spanner.Process(input.ToCharacterSource(), errors);

            // assertions
            Assert.IsEmpty(errors);
            Assert.IsNotNull(spans);
            Assert.AreEqual(1, spans.Length);
            var s = spans[0];
            Assert.AreSame(sequenceDef, s.DefRef);
            Assert.AreEqual(1, s.Subspans.Count);
            var s2 = s.Subspans[0];
            Assert.AreSame(lettersDef, s2.DefRef);
            Assert.AreEqual(3, s2.Subspans.Count);

            Assert.AreSame(lettersDef.Nodes[0], s2.Subspans[0].Node);
            Assert.AreSame(lettersDef.Nodes[0], s2.Subspans[1].Node);
            Assert.AreSame(lettersDef.Nodes[0], s2.Subspans[2].Node);
            Assert.IsEmpty(s2.Subspans[0].Subspans);
            Assert.IsEmpty(s2.Subspans[1].Subspans);
            Assert.IsEmpty(s2.Subspans[2].Subspans);
            Assert.AreEqual("a", s2.Subspans[0].Value);
            Assert.AreEqual("b", s2.Subspans[1].Value);
            Assert.AreEqual("c", s2.Subspans[2].Value);
        }

        [Test]
        public void TestAtomic2()
        {
            // setup
            var sequence =
                new DefinitionExpression(
                    name: "sequence",
                    items: new [] {
                        new DefRefSubExpression("letters", isRepeatable: true)
                    }
                );
            var letters =
                new DefinitionExpression(
                    name: "letters",
                    items: new [] {
                        new CharClassSubExpression(
                            charClass: CharClass.FromUndelimitedCharClassText("\\l"),
                            isRepeatable: true
                        )
                    }
                );
            var defs = (new DefinitionBuilder()).BuildDefinitions(new [] { letters, sequence });
            var grammar = new Grammar(defs);
            var sequenceDef = grammar.FindDefinitionByName("sequence");
            var lettersDef = grammar.FindDefinitionByName("letters");
            var spanner = new Spanner(sequenceDef);
            var input = "abc";
            var errors = new List<Error>();

            // action
            var spans = spanner.Process(input.ToCharacterSource(), errors);

            // assertions
            Assert.IsEmpty(errors);
            Assert.IsNotNull(spans);
            Assert.AreEqual(4, spans.Length);

            // sequence
            //    |
            // letters('abc')

            //       sequence
            //       |      |
            // letters('a') letters('bc')

            //         sequence
            //         |      |
            // letters('ab') letters('c')

            //                sequence
            //              /     |    \
            //             /      |     \
            // letters('a') letters('b') letters('c')

            Assert.AreEqual(1, spans.Count(x => x.Subspans.Count == 1));    // abc
            Assert.AreEqual(1, spans.Count(x => x.Subspans.Count == 3));    // a b c
            Assert.AreEqual(2, spans.Count(x => x.Subspans.Count == 2));
            Assert.AreEqual(1, spans.Count(
                x =>
                x.Subspans.Count == 2 &&
                x.Subspans[0].Subspans.Count == 1 &&
                x.Subspans[1].Subspans.Count == 2));                    // a bc
            Assert.AreEqual(1, spans.Count(
                x =>
                x.Subspans.Count == 2 &&
                x.Subspans[0].Subspans.Count == 2 &&
                x.Subspans[1].Subspans.Count == 1));                    // ab c

            Span s;
            Span s2;

            s = spans.First(x => x.Subspans.Count == 1);    //abc
            Assert.AreSame(sequenceDef, s.DefRef);
            s2 = s.Subspans[0];
            Assert.AreSame(lettersDef, s2.DefRef);
            Assert.AreEqual(3, s2.Subspans.Count);
            Assert.AreSame(lettersDef.Nodes[0], s2.Subspans[0].Node);
            Assert.AreSame(lettersDef.Nodes[0], s2.Subspans[1].Node);
            Assert.AreSame(lettersDef.Nodes[0], s2.Subspans[2].Node);
            Assert.AreEqual("a", s2.Subspans[0].Value);
            Assert.AreEqual("b", s2.Subspans[1].Value);
            Assert.AreEqual("c", s2.Subspans[2].Value);
            Assert.IsEmpty(s2.Subspans[0].Subspans);
            Assert.IsEmpty(s2.Subspans[1].Subspans);
            Assert.IsEmpty(s2.Subspans[2].Subspans);

            s = spans.First(x => x.Subspans.Count == 2 && x.Subspans[0].Subspans.Count == 1); // a bc
            Assert.AreSame(sequenceDef, s.DefRef);
            s2 = s.Subspans[0];
            Assert.AreSame(lettersDef, s2.DefRef);
            Assert.AreEqual(1, s2.Subspans.Count);
            Assert.AreSame(lettersDef.Nodes[0], s2.Subspans[0].Node);
            Assert.AreEqual("a", s2.Subspans[0].Value);
            Assert.IsEmpty(s2.Subspans[0].Subspans);
            s2 = s.Subspans[1];
            Assert.AreSame(lettersDef, s2.DefRef);
            Assert.AreEqual(2, s2.Subspans.Count);
            Assert.AreSame(lettersDef.Nodes[0], s2.Subspans[0].Node);
            Assert.AreSame(lettersDef.Nodes[0], s2.Subspans[1].Node);
            Assert.AreEqual("b", s2.Subspans[0].Value);
            Assert.AreEqual("c", s2.Subspans[1].Value);
            Assert.IsEmpty(s2.Subspans[0].Subspans);
            Assert.IsEmpty(s2.Subspans[1].Subspans);

            s = spans.First(x => x.Subspans.Count == 2 && x.Subspans[0].Subspans.Count == 2); // ab c
            Assert.AreSame(sequenceDef, s.DefRef);
            s2 = s.Subspans[0];
            Assert.AreSame(lettersDef, s2.DefRef);
            Assert.AreEqual(2, s2.Subspans.Count);
            Assert.AreSame(lettersDef.Nodes[0], s2.Subspans[0].Node);
            Assert.AreSame(lettersDef.Nodes[0], s2.Subspans[1].Node);
            Assert.AreEqual("a", s2.Subspans[0].Value);
            Assert.AreEqual("b", s2.Subspans[1].Value);
            Assert.IsEmpty(s2.Subspans[0].Subspans);
            Assert.IsEmpty(s2.Subspans[1].Subspans);
            s2 = s.Subspans[1];
            Assert.AreSame(lettersDef, s2.DefRef);
            Assert.AreEqual(1, s2.Subspans.Count);
            Assert.AreSame(lettersDef.Nodes[0], s2.Subspans[0].Node);
            Assert.AreEqual("c", s2.Subspans[0].Value);
            Assert.IsEmpty(s2.Subspans[0].Subspans);

            s = spans.First(x => x.Subspans.Count == 3);    // a b c
            Assert.AreSame(sequenceDef, s.DefRef);
            Assert.AreSame(lettersDef, s.Subspans[0].DefRef);
            Assert.AreSame(lettersDef, s.Subspans[1].DefRef);
            Assert.AreSame(lettersDef, s.Subspans[2].DefRef);
            Assert.AreEqual(1, s.Subspans[0].Subspans.Count);
            Assert.AreEqual(1, s.Subspans[1].Subspans.Count);
            Assert.AreEqual(1, s.Subspans[2].Subspans.Count);
            Assert.AreSame(lettersDef.Nodes[0], s.Subspans[0].Subspans[0].Node);
            Assert.AreSame(lettersDef.Nodes[0], s.Subspans[1].Subspans[0].Node);
            Assert.AreSame(lettersDef.Nodes[0], s.Subspans[2].Subspans[0].Node);
            Assert.AreEqual("a", s.Subspans[0].Subspans[0].Value);
            Assert.AreEqual("b", s.Subspans[1].Subspans[0].Value);
            Assert.AreEqual("c", s.Subspans[2].Subspans[0].Value);
            Assert.IsEmpty(s.Subspans[0].Subspans[0].Subspans);
            Assert.IsEmpty(s.Subspans[1].Subspans[0].Subspans);
            Assert.IsEmpty(s.Subspans[2].Subspans[0].Subspans);
        }




        /////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////



        [Test]
        public void TestAtomicOffWithIntermediateDefRefs()
        {
            var grammarFile = 
                "sequence = item+;\n" +
                "item = name;\n" +
                "name = [\\l]+;";
            var errors = new List<Error>();
            var grammar = (new SupergrammarSpanner()).GetGrammar(grammarFile, errors);
            Assert.IsEmpty(errors);
            var spanner = new Spanner(grammar.FindDefinitionByName("sequence"));
            var input = "abcd";

            var spans = spanner.Process(input.ToCharacterSource(), errors);

            Assert.IsEmpty(errors);

            var sb = new StringBuilder();
            int k = 0;
            foreach (var span in spans)
            {
                sb.AppendFormat("Span {0}", k);
                sb.AppendLine();
                sb.AppendLine(span.RenderSpanHierarchy());
                sb.AppendLine();
                sb.AppendLine();
                k++;
            }

            Console.WriteLine("Spans for {0}", TestContext.CurrentContext.Test.FullName);
            Console.WriteLine(grammarFile);
            Console.WriteLine(sb.ToString());
        }

        [Test]
        public void TestAtomicOnWithIntermediateDefRefs()
        {
            var grammarFile = 
                "sequence = item+;\n" +
                "item = name;\n" +
                "<atomic> name = [\\l]+;";
            var errors = new List<Error>();
            var grammar = (new SupergrammarSpanner()).GetGrammar(grammarFile, errors);
            Assert.IsEmpty(errors);
            var spanner = new Spanner(grammar.FindDefinitionByName("sequence"));
            var input = "abcd";

            var spans = spanner.Process(input.ToCharacterSource(), errors);

            Assert.IsEmpty(errors);

            var sb = new StringBuilder();
            int k = 0;
            foreach (var span in spans)
            {
                sb.AppendFormat("Span {0}", k);
                sb.AppendLine();
                sb.AppendLine(span.RenderSpanHierarchy());
                sb.AppendLine();
                sb.AppendLine();
                k++;
            }

            Console.WriteLine("Spans for {0}", TestContext.CurrentContext.Test.FullName);
            Console.WriteLine(grammarFile);
            Console.WriteLine(sb.ToString());
        }

        [Test]
        public void TestAtomicOffWithAtomicIntermediateDefRefs()
        {
            var grammarFile = 
                "sequence = item+;\n" +
                "<atomic> item = name;\n" +
                "name = [\\l]+;";
            var errors = new List<Error>();
            var grammar = (new SupergrammarSpanner()).GetGrammar(grammarFile, errors);
            Assert.IsEmpty(errors);
            var spanner = new Spanner(grammar.FindDefinitionByName("sequence"));
            var input = "abcd";

            var spans = spanner.Process(input.ToCharacterSource(), errors);

            Assert.IsEmpty(errors);

            var sb = new StringBuilder();
            int k = 0;
            foreach (var span in spans)
            {
                sb.AppendFormat("Span {0}", k);
                sb.AppendLine();
                sb.AppendLine(span.RenderSpanHierarchy());
                sb.AppendLine();
                sb.AppendLine();
                k++;
            }

            Console.WriteLine("Spans for {0}", TestContext.CurrentContext.Test.FullName);
            Console.WriteLine(grammarFile);
            Console.WriteLine(sb.ToString());
        }

        [Test]
        public void TestAtomicOnWithAtomicIntermediateDefRefs()
        {
            var grammarFile = 
                "sequence = item+;\n" +
                "<atomic> item = name;\n" +
                "<atomic> name = [\\l]+;";
            var errors = new List<Error>();
            var grammar = (new SupergrammarSpanner()).GetGrammar(grammarFile, errors);
            Assert.IsEmpty(errors);
            var spanner = new Spanner(grammar.FindDefinitionByName("sequence"));
            var input = "abcd";

            var spans = spanner.Process(input.ToCharacterSource(), errors);

            Assert.IsEmpty(errors);

            var sb = new StringBuilder();
            int k = 0;
            foreach (var span in spans)
            {
                sb.AppendFormat("Span {0}", k);
                sb.AppendLine();
                sb.AppendLine(span.RenderSpanHierarchy());
                sb.AppendLine();
                sb.AppendLine();
                k++;
            }

            Console.WriteLine("Spans for {0}", TestContext.CurrentContext.Test.FullName);
            Console.WriteLine(grammarFile);
            Console.WriteLine(sb.ToString());
        }





        /////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////




        static void DoWhitespaceTest(string grammarFile)
        {
            var errors = new List<Error>();
            var grammar = (new SupergrammarSpanner()).GetGrammar(grammarFile, errors);
            Assert.IsEmpty(errors);
            var spanner = new Spanner(grammar.FindDefinitionByName("sequence"));
            var input = "ab cd";

            var spans = spanner.Process(input.ToCharacterSource(), errors);

            ReportResults(grammarFile, input, spans, errors);
            Assert.IsEmpty(errors);
        }




        [Test]
        public void TestNormalLowerWithNormalIntermediateDefRefsWithInputWhitespace()
        {
            var grammarFile =
                "sequence = item+;\n" +
                "item = name;\n" +
                "name = [\\l]+;";
            DoWhitespaceTest(grammarFile);
        }

        [Test]
        public void TestNormalLowerWithMindWhitespaceIntermediateDefRefsWithInputWhitespace()
        {
            var grammarFile =
                "sequence = item+;\n" +
                "<mind whitespace> item = name;\n" +
                "name = [\\l]+;";
            DoWhitespaceTest(grammarFile);
        }

        [Test]
        public void TestNormalLowerWithAtomicIntermediateDefRefsWithInputWhitespace()
        {
            var grammarFile =
                "sequence = item+;\n" +
                "<atomic> item = name;\n" +
                "name = [\\l]+;";
            DoWhitespaceTest(grammarFile);
        }

        [Test]
        public void TestNormalLowerWithAtomicMindWhitespaceIntermediateDefRefsWithInputWhitespace()
        {
            var grammarFile =
                "sequence = item+;\n" +
                "<atomic, mind whitespace> item = name;\n" +
                "name = [\\l]+;";
            DoWhitespaceTest(grammarFile);
        }

        [Test]
        public void TestMindWhitespaceLowerWithNormalIntermediateDefRefsWithInputWhitespace()
        {
            var grammarFile =
                "sequence = item+;\n" +
                "item = name;\n" +
                "<mind whitespace> name = [\\l]+;";
            DoWhitespaceTest(grammarFile);
        }

        [Test]
        public void TestMindWhitespaceLowerWithMindWhitespaceIntermediateDefRefsWithInputWhitespace()
        {
            var grammarFile =
                "sequence = item+;\n" +
                "<mind whitespace> item = name;\n" +
                "<mind whitespace> name = [\\l]+;";
            DoWhitespaceTest(grammarFile);
        }

        [Test]
        public void TestMindWhitespaceLowerWithAtomicIntermediateDefRefsWithInputWhitespace()
        {
            var grammarFile =
                "sequence = item+;\n" +
                "<atomic> item = name;\n" +
                "<mind whitespace> name = [\\l]+;";
            DoWhitespaceTest(grammarFile);
        }

        [Test]
        public void TestMindWhitespaceLowerWithAtomicMindWhitespaceIntermediateDefRefsWithInputWhitespace()
        {
            var grammarFile =
                "sequence = item+;\n" +
                "<atomic, mind whitespace> item = name;\n" +
                "<mind whitespace> name = [\\l]+;";
            DoWhitespaceTest(grammarFile);
        }

        [Test]
        public void TestAtomicLowerWithNormalIntermediateDefRefsWithInputWhitespace()
        {
            var grammarFile =
                "sequence = item+;\n" +
                "item = name;\n" +
                "<atomic> name = [\\l]+;";
            DoWhitespaceTest(grammarFile);
        }

        [Test]
        public void TestAtomicLowerWithMindWhitespaceIntermediateDefRefsWithInputWhitespace()
        {
            var grammarFile =
                "sequence = item+;\n" +
                "<mind whitespace> item = name;\n" +
                "<atomic> name = [\\l]+;";
            DoWhitespaceTest(grammarFile);
        }

        [Test]
        public void TestAtomicLowerWithAtomicIntermediateDefRefsWithInputWhitespace()
        {
            var grammarFile =
                "sequence = item+;\n" +
                "<atomic> item = name;\n" +
                "<atomic> name = [\\l]+;";
            DoWhitespaceTest(grammarFile);
        }

        [Test]
        public void TestAtomicLowerWithAtomicMindWhitespaceIntermediateDefRefsWithInputWhitespace()
        {
            var grammarFile =
                "sequence = item+;\n" +
                "<atomic, mind whitespace> item = name;\n" +
                "<atomic> name = [\\l]+;";
            DoWhitespaceTest(grammarFile);
        }

        [Test]
        public void TestAtomicMindWhitespaceLowerWithNormalIntermediateDefRefsWithInputWhitespace()
        {
            var grammarFile =
                "sequence = item+;\n" +
                "item = name;\n" +
                "<atomic, mind whitespace> name = [\\l]+;";
            DoWhitespaceTest(grammarFile);
        }

        [Test]
        public void TestAtomicMindWhitespaceLowerWithMindWhitespaceIntermediateDefRefsWithInputWhitespace()
        {
            var grammarFile =
                "sequence = item+;\n" +
                "<mind whitespace> item = name;\n" +
                "<atomic, mind whitespace> name = [\\l]+;";
            DoWhitespaceTest(grammarFile);
        }

        [Test]
        public void TestAtomicMindWhitespaceLowerWithAtomicIntermediateDefRefsWithInputWhitespace()
        {
            var grammarFile =
                "sequence = item+;\n" +
                "<atomic> item = name;\n" +
                "<atomic, mind whitespace> name = [\\l]+;";
            DoWhitespaceTest(grammarFile);
        }

        [Test]
        public void TestAtomicMindWhitespaceLowerWithAtomicMindWhitespaceIntermediateDefRefsWithInputWhitespace()
        {
            var grammarFile =
                "sequence = item+;\n" +
                "<atomic, mind whitespace> item = name;\n" +
                "<atomic, mind whitespace> name = [\\l]+;";
            DoWhitespaceTest(grammarFile);
        }



        /////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////





        [Test]
        public void TestTrailingLiteralAfterAtomicDefRef()
        {
            var grammarFile = 
                "sequence = name+ '-efg';\n" +
                "<atomic> name = [\\l]+;";
            var errors = new List<Error>();
            var grammar = (new SupergrammarSpanner()).GetGrammar(grammarFile, errors);
            Assert.IsEmpty(errors);
            var spanner = new Spanner(grammar.FindDefinitionByName("sequence"));
            var input = "abcdefg";

            var spans = spanner.Process(input.ToCharacterSource(), errors);

            ReportResults(grammarFile, input, spans, errors);

            Assert.IsEmpty(errors);
        }

        [Test]
        public void TestAtomicDefWithoutRepatingModifier()
        {
            var grammarFile =
                "sequence = item+; \n" +
                "<atomic> item = 'a' ('b' 'c'?)?;";
            var errors = new List<Error>();
            var grammar = (new SupergrammarSpanner()).GetGrammar(grammarFile, errors);
            Assert.IsEmpty(errors);
            var sequenceDef = grammar.FindDefinitionByName("sequence");
            var itemDef = grammar.FindDefinitionByName("item");
            var spanner = new Spanner(sequenceDef);
            var input = "abcabc";

            var spans = spanner.Process(input.ToCharacterSource(), errors);

            Assert.IsEmpty(errors);
            Assert.AreEqual(1, spans.Length);
            Assert.AreEqual(
                new Span(
                    node: sequenceDef.Nodes[0],
                    subspans: 
        }

        public static void ReportResults(string grammarFile, string input, Span[] spans, List<Error> errors)
        {
            Console.WriteLine("Results for {0}", TestContext.CurrentContext.Test.Name);
            Console.WriteLine(grammarFile);
            Console.WriteLine("Input: \"{0}\"", input);
            Console.WriteLine("{0} spans", spans.Length);
            Console.WriteLine(
                "{0} warnings and {1} non-warnings, {2} total",
                errors.GetWarningsCount(),
                errors.GetNonWarningsCount(),
                errors.Count);

            int k = 0;
            foreach (var error in errors)
            {
                Console.WriteLine("Error {0}: {1}", k, error.Description);
                k++;
            }

            k = 0;
            foreach (var span in spans)
            {
                Console.WriteLine("Span {0}", k);
                Console.WriteLine(span.RenderSpanHierarchyAsCSharpExpression(null));
                Console.WriteLine();
                k++;
            }
        }

    }
}

