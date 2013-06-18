using System;
using NUnit.Framework;

namespace MetaphysicsIndustries.Giza.Test
{
    [TestFixture()]
    public class SpannerTest
    {
        [Test]
        public void TestOrExpr()
        {
            string testGrammarText = 
                "def1 = ( 'qwer' | 'asdf' );";
            string error;
            Grammar testGrammar = (new SupergrammarSpanner()).GetGrammar(testGrammarText, out error);
            if (!string.IsNullOrEmpty(error))
            {
                throw new InvalidOperationException(error);
            }

            Spanner s = new Spanner();

            Span[] spans = s.Process(testGrammar, "def1", "qwer", out error);
            Assert.AreEqual(1, spans.Length);
            Assert.IsNull(error);

            spans = s.Process(testGrammar, "def1", "asdf", out error);
            Assert.AreEqual(1, spans.Length);
            Assert.IsNull(error);
        }

        [Test()]
        public void TestErrorInvalidCharacterAfterDefRef()
        {
            string testGrammarText = 
                " // test grammar \r\n" +
                "sequence = item+; \r\n" +
                "item = ( id-item1 | id-item2 ); \r\n" +
                "<whitespace, atomic> id-item1 = 'item1'; \r\n" +
                "<whitespace, atomic> id-item2 = 'item2'; \r\n";

            Spanner s = new Spanner();
            Supergrammar sg = new Supergrammar();
            string error;
            Span[] spans = s.Process(sg.def_0_grammar, testGrammarText, out error);

            Assert.IsEmpty(spans);
            Assert.AreEqual("Invalid character 'w' at (4,2), after a '<': expected id-mind, id-ignore, id-atomic, or id-token", error);
        }

        [Test()]
        public void TestErrorInvalidCharacterAtStart()
        {
            string testGrammarText = 
                " // test grammar \r\n" +
                "sequence = item+; \r\n" +
                "item = ( id-item1 | id-item2 ); \r\n" +
                "<mind whitespace, atomic> id-item1 = 'item1'; \r\n" +
                "<mind whitespace, atomic> id-item2 = 'item2'; \r\n";

            string testInput = "$ item1 item2 ";

            SupergrammarSpanner sgs = new SupergrammarSpanner();
            string error1;
            Grammar testGrammar = sgs.GetGrammar(testGrammarText, out error1);
            Spanner s = new Spanner();
            string error;
            Span[] spans = s.Process(testGrammar, "sequence", testInput, out error);

            Assert.IsEmpty(spans);
            Assert.AreEqual("Invalid character '$' at (1,1): a sequence must start with item", error);
        }

    }
}
