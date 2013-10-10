using System;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace MetaphysicsIndustries.Giza.Test
{
    [TestFixture()]
    public class TokenizerTest
    {
        [Test()]
        public void TestNormal()
        {
            string testGrammarText =
                " // test grammar \r\n" +
                "sequence = item+; \r\n" +
                "item = ( id-item1 | id-item2 ); \r\n" +
                "<mind whitespace, atomic, token> id-item1 = 'item1'; \r\n" +
                "<mind whitespace, atomic, token> id-item2 = 'item2'; \r\n";

            var errors = new List<Error>();
            DefinitionExpression[] dis = (new SupergrammarSpanner()).GetExpressions(testGrammarText, errors);
            Assert.IsEmpty(errors);

            Grammar testGrammar = (new TokenizedGrammarBuilder()).BuildTokenizedGrammar(dis);
            Definition item1Def = testGrammar.FindDefinitionByName("id-item1");
            Definition item2Def = testGrammar.FindDefinitionByName("id-item2");

            Tokenizer t = new Tokenizer(testGrammar, "item1 item2");
            bool endOfInput;
            InputPosition endOfInputPosition;
            var tokens = t.GetTokensAtLocation(0, errors,
                                               out endOfInput, out endOfInputPosition);
            Assert.IsEmpty(errors);
            Assert.IsFalse(endOfInput);
            Assert.AreEqual(1, tokens.Count());
            var first = tokens.First();
            Assert.AreEqual(item1Def, first.Definition);
            Assert.AreEqual(0, first.StartIndex);
            Assert.AreEqual("item1", first.Value);

            tokens = t.GetTokensAtLocation(5, errors,
                                           out endOfInput, out endOfInputPosition);
            Assert.IsEmpty(errors);
            Assert.IsFalse(endOfInput);
            Assert.AreEqual(1, tokens.Count());
            first = tokens.First();
            Assert.AreEqual(item2Def, first.Definition);
            Assert.AreEqual(6, first.StartIndex);
            Assert.AreEqual("item2", first.Value);
        }

        [Test()]
        public void TestAmbiguousSeparateTokens()
        {
            string testGrammarText =
                " // test grammar \r\n" +
                "expr = item ( oper item )+; \r\n" +
                "item = ( varref | prefix | postfix ); \r\n" +
                "prefix = plusplus varref; \r\n" +
                "postfix = varref plusplus; \r\n" +
                "<mind whitespace, atomic, token> varref = [\\l]+ ; \r\n" +
                "<mind whitespace, token> plusplus = '++'; \r\n" +
                "<token> oper = '+'; \r\n";

            string testInput = "a+++b";

            var errors = new List<Error>();
            DefinitionExpression[] dis = (new SupergrammarSpanner()).GetExpressions(testGrammarText, errors);
            Assert.IsEmpty(errors);

            Grammar testGrammar = (new TokenizedGrammarBuilder()).BuildTokenizedGrammar(dis);
            Definition varrefDef = testGrammar.FindDefinitionByName("varref");
            Definition plusplusDef = testGrammar.FindDefinitionByName("plusplus");
            Definition operDef = testGrammar.FindDefinitionByName("oper");

            Tokenizer t = new Tokenizer(testGrammar, testInput);
            bool endOfInput;
            InputPosition endOfInputPosition;

            var tokens = t.GetTokensAtLocation(0, errors,
                                               out endOfInput, out endOfInputPosition);
            Assert.IsEmpty(errors);
            Assert.IsFalse(endOfInput);
            Assert.AreEqual(1, tokens.Count());
            var first = tokens.First();
            Assert.AreEqual(varrefDef, first.Definition);
            Assert.AreEqual(0, first.StartIndex);
            Assert.AreEqual("a", first.Value);

            tokens = t.GetTokensAtLocation(1, errors,
                                           out endOfInput, out endOfInputPosition);
            Assert.IsEmpty(errors);
            Assert.IsFalse(endOfInput);
            Assert.AreEqual(2, tokens.Count());
            first = tokens.First();
            var second = tokens.ElementAt(1);
            Assert.IsTrue(first.Definition == plusplusDef || first.Definition == operDef);
            Assert.IsTrue(second.Definition == plusplusDef || second.Definition == operDef);
            Assert.IsFalse(first.Definition == second.Definition);

            Token plusplusToken;
            Token operToken;
            if (first.Definition == plusplusDef)
            {
                plusplusToken = first;
                operToken = second;
            }
            else
            {
                plusplusToken = second;
                operToken = first;
            }

            Assert.AreEqual(1, plusplusToken.StartIndex);
            Assert.AreEqual("++", plusplusToken.Value);
            Assert.AreEqual(1, operToken.StartIndex);
            Assert.AreEqual("+", operToken.Value);
        }

        [Test()]
        public void TestAmbiguousCombinedToken()
        {
            string testGrammarText =
                " // test grammar \r\n" +
                "expr = item ( oper item )+; \r\n" +
                "<mind whitespace, atomic, token> item = [\\l]+ ; \r\n" +
                "<mind whitespace, token> oper = ( '<' | '<<' ); \r\n";

            string testInput = "a << b";

            var errors = new List<Error>();
            DefinitionExpression[] dis = (new SupergrammarSpanner()).GetExpressions(testGrammarText, errors); 
            Assert.IsEmpty(errors);
            TokenizedGrammarBuilder tgb = new TokenizedGrammarBuilder();
            Grammar testGrammar = tgb.BuildTokenizedGrammar(dis);


            Definition itemDef = testGrammar.FindDefinitionByName("item");
            Definition operDef = testGrammar.FindDefinitionByName("oper");

            Tokenizer t = new Tokenizer(testGrammar, testInput);
            bool endOfInput;
            InputPosition endOfInputPosition;

            var tokens = t.GetTokensAtLocation(0, errors,
                                               out endOfInput, out endOfInputPosition);
            Assert.IsEmpty(errors);
            Assert.IsFalse(endOfInput);
            Assert.AreEqual(1, tokens.Count());
            var first = tokens.First();
            Assert.AreEqual(itemDef, first.Definition);
            Assert.AreEqual(0, first.StartIndex);
            Assert.AreEqual("a", first.Value);

            tokens = t.GetTokensAtLocation(1, errors,
                                           out endOfInput, out endOfInputPosition);
            Assert.IsEmpty(errors);
            Assert.IsFalse(endOfInput);
            Assert.AreEqual(2, tokens.Count());
            first = tokens.First();
            var second = tokens.ElementAt(1);
            Assert.AreEqual(2, first.StartIndex);
            Assert.AreEqual(2, second.StartIndex);
            Assert.AreEqual(operDef, first.Definition);
            Assert.AreEqual(operDef, second.Definition);
            Assert.IsTrue(first.Value == "<" || first.Value == "<<");
            Assert.IsTrue(second.Value == "<" || second.Value == "<<");
            Assert.IsTrue(first.Value != second.Value);
        }

        [Test]
        public void TestTokensAtIndex0()
        {
            string grammarText =
                "expr = operand '+' operand;\n" +
                "<token> operand = [\\l_] [\\l\\d_]*;";

            var errors = new List<Error>();
            DefinitionExpression[] dis = (new SupergrammarSpanner()).GetExpressions(grammarText, errors);
            Assert.IsEmpty(errors);

            Grammar grammar = (new TokenizedGrammarBuilder()).BuildTokenizedGrammar(dis);

            Definition operandDef = grammar.FindDefinitionByName("operand");

            string input = "a + b";
            Tokenizer tokenizer = new Tokenizer(grammar, input);
            bool endOfInput;
            InputPosition endOfInputPosition;


            var tokens = tokenizer.GetTokensAtLocation(0, errors,
                                                       out endOfInput,
                                                       out endOfInputPosition);

            Assert.IsEmpty(errors);
            Assert.IsFalse(endOfInput);
            Assert.IsNotNull(tokens);
            Assert.AreEqual(1, tokens.Count());
            var first = tokens.First();
            Assert.AreSame(operandDef, first.Definition);
            Assert.AreEqual(0, first.StartIndex);
            Assert.AreEqual("a", first.Value);
            Assert.AreEqual(-1, endOfInputPosition.Index);
        }

        [Test]
        public void TestTokensAtIndex1()
        {
            string grammarText =
                "expr = operand '+' operand;\n" +
                "<token> operand = [\\l_] [\\l\\d_]*;";

            var errors = new List<Error>();
            DefinitionExpression[] dis = (new SupergrammarSpanner()).GetExpressions(grammarText, errors);
            Assert.IsEmpty(errors);

            Grammar grammar = (new TokenizedGrammarBuilder()).BuildTokenizedGrammar(dis);

            Definition operatorDef = grammar.FindDefinitionByName("$implicit literal +");

            string input = "a + b";
            Tokenizer tokenizer = new Tokenizer(grammar, input);
            bool endOfInput;
            InputPosition endOfInputPosition;


            var tokens = tokenizer.GetTokensAtLocation(1, errors,
                                                       out endOfInput,
                                                       out endOfInputPosition);

            Assert.IsEmpty(errors);
            Assert.IsFalse(endOfInput);
            Assert.IsNotNull(tokens);
            Assert.AreEqual(1, tokens.Count());
            var first = tokens.First();
            Assert.AreSame(operatorDef, first.Definition);
            Assert.AreEqual(2, first.StartIndex);
            Assert.AreEqual("+", first.Value);
            Assert.AreEqual(-1, endOfInputPosition.Index);
        }

        [Test]
        public void TestTokensAtIndex2()
        {
            string grammarText =
                "expr = operand '+' operand;\n" +
                "<token> operand = [\\l_] [\\l\\d_]*;";

            var errors = new List<Error>();
            DefinitionExpression[] dis = (new SupergrammarSpanner()).GetExpressions(grammarText, errors);
            Assert.IsEmpty(errors);

            Grammar grammar = (new TokenizedGrammarBuilder()).BuildTokenizedGrammar(dis);

            Definition operatorDef = grammar.FindDefinitionByName("$implicit literal +");

            string input = "a + b";
            Tokenizer tokenizer = new Tokenizer(grammar, input);
            bool endOfInput;
            InputPosition endOfInputPosition;


            var tokens = tokenizer.GetTokensAtLocation(2, errors,
                                                       out endOfInput,
                                                       out endOfInputPosition);

            Assert.IsEmpty(errors);
            Assert.IsFalse(endOfInput);
            Assert.IsNotNull(tokens);
            Assert.AreEqual(1, tokens.Count());
            var first = tokens.First();
            Assert.AreSame(operatorDef, first.Definition);
            Assert.AreEqual(2, first.StartIndex);
            Assert.AreEqual("+", first.Value);
            Assert.AreEqual(-1, endOfInputPosition.Index);
        }

        [Test]
        public void TestTokensAtIndex3()
        {
            string grammarText =
                "expr = operand '+' operand;\n" +
                "<token> operand = [\\l_] [\\l\\d_]*;";

            var errors = new List<Error>();
            DefinitionExpression[] dis = (new SupergrammarSpanner()).GetExpressions(grammarText, errors);
            Assert.IsEmpty(errors);

            Grammar grammar = (new TokenizedGrammarBuilder()).BuildTokenizedGrammar(dis);

            Definition operandDef = grammar.FindDefinitionByName("operand");

            string input = "a + b";
            Tokenizer tokenizer = new Tokenizer(grammar, input);
            bool endOfInput;
            InputPosition endOfInputPosition;


            var tokens = tokenizer.GetTokensAtLocation(3, errors,
                                                   out endOfInput,
                                                   out endOfInputPosition);

            Assert.IsEmpty(errors);
            Assert.IsFalse(endOfInput);
            Assert.IsNotNull(tokens);
            Assert.AreEqual(1, tokens.Count());
            var first = tokens.First();
            Assert.AreSame(operandDef, first.Definition);
            Assert.AreEqual(4, first.StartIndex);
            Assert.AreEqual("b", first.Value);
            Assert.AreEqual(-1, endOfInputPosition.Index);
        }

        [Test]
        public void TestTokensAtIndex4()
        {
            string grammarText =
                "expr = operand '+' operand;\n" +
                    "<token> operand = [\\l_] [\\l\\d_]*;";

            var errors = new List<Error>();
            DefinitionExpression[] dis = (new SupergrammarSpanner()).GetExpressions(grammarText, errors);
            Assert.IsEmpty(errors);

            Grammar grammar = (new TokenizedGrammarBuilder()).BuildTokenizedGrammar(dis);

            Definition operandDef = grammar.FindDefinitionByName("operand");

            string input = "a + b";
            Tokenizer tokenizer = new Tokenizer(grammar, input);
            bool endOfInput;
            InputPosition endOfInputPosition;


            var tokens = tokenizer.GetTokensAtLocation(4, errors,
                                                   out endOfInput,
                                                   out endOfInputPosition);

            Assert.IsEmpty(errors);
            Assert.IsFalse(endOfInput);
            Assert.IsNotNull(tokens);
            Assert.AreEqual(1, tokens.Count());
            var first = tokens.First();
            Assert.AreSame(operandDef, first.Definition);
            Assert.AreEqual(4, first.StartIndex);
            Assert.AreEqual("b", first.Value);
            Assert.AreEqual(-1, endOfInputPosition.Index);
        }

        [Test]
        public void TestTokensAtIndex5()
        {
            string grammarText =
                "expr = operand '+' operand;\n" +
                    "<token> operand = [\\l_] [\\l\\d_]*;";

            var errors = new List<Error>();
            DefinitionExpression[] dis = (new SupergrammarSpanner()).GetExpressions(grammarText, errors);
            Assert.IsEmpty(errors);

            Grammar grammar = (new TokenizedGrammarBuilder()).BuildTokenizedGrammar(dis);

            string input = "a + b";
            Tokenizer tokenizer = new Tokenizer(grammar, input);
            bool endOfInput;
            InputPosition endOfInputPosition;


            var tokens = tokenizer.GetTokensAtLocation(5, errors,
                                                   out endOfInput,
                                                   out endOfInputPosition);

            Assert.IsEmpty(errors);
            Assert.IsTrue(endOfInput);
            Assert.IsNotNull(tokens);
            Assert.AreEqual(0, tokens.Count());
            Assert.AreEqual(5, endOfInputPosition.Index);
        }

        [Test]
        public void TestEndOfInputParameter1()
        {
            string grammarText =
                "expr = operand '+' operand;\n" +
                "<token> operand = [\\l_] [\\l\\d_]*;";

            var errors = new List<Error>();
            DefinitionExpression[] dis = (new SupergrammarSpanner()).GetExpressions(grammarText, errors);
            Assert.IsEmpty(errors);

            Grammar grammar = (new TokenizedGrammarBuilder()).BuildTokenizedGrammar(dis);
            string input = "a + b";
            Tokenizer tokenizer = new Tokenizer(grammar, input);
            bool endOfInput;
            InputPosition endOfInputPosition;


            var tokens = tokenizer.GetTokensAtLocation(5, errors,
                                                       out endOfInput,
                                                       out endOfInputPosition);


            Assert.IsTrue(endOfInput);
            Assert.IsNotNull(tokens);
            Assert.IsEmpty(tokens);
            Assert.IsNotNull(errors);
            Assert.IsEmpty(errors);
            Assert.AreEqual(5, endOfInputPosition.Index);
        }

        [Test]
        public void TestEndOfInputParameter2()
        {
            string grammarText =
                "expr = operand '+' operand;\n" +
                    "<token> operand = [\\l_] [\\l\\d_]*;";

            var errors = new List<Error>();
            DefinitionExpression[] dis = (new SupergrammarSpanner()).GetExpressions(grammarText, errors);
            Assert.IsEmpty(errors);

            Grammar grammar = (new TokenizedGrammarBuilder()).BuildTokenizedGrammar(dis);
            string input = "a + b ";
            Tokenizer tokenizer = new Tokenizer(grammar, input);
            bool endOfInput;
            InputPosition endOfInputPosition;


            var tokens = tokenizer.GetTokensAtLocation(5, errors,
                                                       out endOfInput,
                                                       out endOfInputPosition);


            Assert.IsTrue(endOfInput);
            Assert.IsNotNull(tokens);
            Assert.IsEmpty(tokens);
            Assert.IsNotNull(errors);
            Assert.IsEmpty(errors);
            Assert.AreEqual(6, endOfInputPosition.Index);
        }

        [Test]
        public void TestEndOfInputParameter3()
        {
            string grammarText =
                "expr = operand '+' operand;\n" +
                    "<token> operand = [\\l_] [\\l\\d_]*;";

            var errors = new List<Error>();
            DefinitionExpression[] dis = (new SupergrammarSpanner()).GetExpressions(grammarText, errors);
            Assert.IsEmpty(errors);

            Grammar grammar = (new TokenizedGrammarBuilder()).BuildTokenizedGrammar(dis);
            string input = "a + b ";
            Tokenizer tokenizer = new Tokenizer(grammar, input);
            bool endOfInput;
            InputPosition endOfInputPosition;


            var tokens = tokenizer.GetTokensAtLocation(6, errors,
                                                       out endOfInput,
                                                       out endOfInputPosition);


            Assert.IsTrue(endOfInput);
            Assert.IsNotNull(tokens);
            Assert.IsEmpty(tokens);
            Assert.IsNotNull(errors);
            Assert.IsEmpty(errors);
            Assert.AreEqual(6, endOfInputPosition.Index);
        }

        [Test]
        public void TestEndOfInputParameterWithComment1()
        {
            string grammarText =
                "expr = operand '+' operand;\n" +
                "<token> operand = [\\l_] [\\l\\d_]*;\n" +
                "<comment> comment = '/*' [^*]* '*/';";

            var errors = new List<Error>();
            DefinitionExpression[] dis = (new SupergrammarSpanner()).GetExpressions(grammarText, errors);
            Assert.IsEmpty(errors);

            Grammar grammar = (new TokenizedGrammarBuilder()).BuildTokenizedGrammar(dis);
            string input = "a + b/*comment*/";
            Tokenizer tokenizer = new Tokenizer(grammar, input);
            bool endOfInput;
            InputPosition endOfInputPosition;


            var tokens = tokenizer.GetTokensAtLocation(5, errors,
                                                       out endOfInput,
                                                       out endOfInputPosition);


            Assert.IsTrue(endOfInput);
            Assert.IsNotNull(tokens);
            Assert.IsEmpty(tokens);
            Assert.IsNotNull(errors);
            Assert.IsEmpty(errors);
            Assert.AreEqual(16, endOfInputPosition.Index);
        }

        [Test]
        public void TestEndOfInputParameterWithComment2()
        {
            string grammarText =
                "expr = operand '+' operand;\n" +
                    "<token> operand = [\\l_] [\\l\\d_]*;\n" +
                    "<comment> comment = '/*' [^*]* '*/';";

            var errors = new List<Error>();
            DefinitionExpression[] dis = (new SupergrammarSpanner()).GetExpressions(grammarText, errors);
            Assert.IsEmpty(errors);

            Grammar grammar = (new TokenizedGrammarBuilder()).BuildTokenizedGrammar(dis);
            string input = "a + b /*comment*/";
            Tokenizer tokenizer = new Tokenizer(grammar, input);
            bool endOfInput;
            InputPosition endOfInputPosition;


            var tokens = tokenizer.GetTokensAtLocation(5, errors,
                                                       out endOfInput,
                                                       out endOfInputPosition);


            Assert.IsTrue(endOfInput);
            Assert.IsNotNull(tokens);
            Assert.IsEmpty(tokens);
            Assert.IsNotNull(errors);
            Assert.IsEmpty(errors);
            Assert.AreEqual(17, endOfInputPosition.Index);
        }

        [Test]
        public void TestEndOfInputParameterWithComment3()
        {
            string grammarText =
                "expr = operand '+' operand;\n" +
                    "<token> operand = [\\l_] [\\l\\d_]*;\n" +
                    "<comment> comment = '/*' [^*]* '*/';";

            var errors = new List<Error>();
            DefinitionExpression[] dis = (new SupergrammarSpanner()).GetExpressions(grammarText, errors);
            Assert.IsEmpty(errors);

            Grammar grammar = (new TokenizedGrammarBuilder()).BuildTokenizedGrammar(dis);
            string input = "a + b/*comment*/ ";
            Tokenizer tokenizer = new Tokenizer(grammar, input);
            bool endOfInput;
            InputPosition endOfInputPosition;


            var tokens = tokenizer.GetTokensAtLocation(5, errors,
                                                       out endOfInput,
                                                       out endOfInputPosition);


            Assert.IsTrue(endOfInput);
            Assert.IsNotNull(tokens);
            Assert.IsEmpty(tokens);
            Assert.IsNotNull(errors);
            Assert.IsEmpty(errors);
            Assert.AreEqual(17, endOfInputPosition.Index);
        }

        [Test]
        public void TestEndOfInputParameterWithAmbiguousComment()
        {
            string grammarText =
                "expr = operand '+' operand;\n" +
                    "<token> operand = [\\l_] [\\l\\d_]*;\n" +
                    "<comment> comment = '/*' [^*]* '*/';\n" +
                    "<token> strange = '/*' [\\l]+;\n";

            var errors = new List<Error>();
            DefinitionExpression[] dis = (new SupergrammarSpanner()).GetExpressions(grammarText, errors);
            Assert.IsEmpty(errors);

            Grammar grammar = (new TokenizedGrammarBuilder()).BuildTokenizedGrammar(dis);
            Definition strangeDef = grammar.FindDefinitionByName("strange");
            string input = "a + b/*comment*/";
            Tokenizer tokenizer = new Tokenizer(grammar, input);
            bool endOfInput;
            InputPosition endOfInputPosition;


            var tokens = tokenizer.GetTokensAtLocation(5, errors,
                                                       out endOfInput,
                                                       out endOfInputPosition);


            Assert.IsTrue(endOfInput);
            Assert.IsNotNull(errors);
            Assert.IsEmpty(errors);
            Assert.IsNotNull(tokens);
            Assert.AreEqual(1, tokens.Count());
            var first = tokens.First();
            Assert.AreSame(strangeDef, first.Definition);
            Assert.AreEqual(5, first.StartIndex);
            Assert.AreEqual("/*comment", first.Value);
            Assert.AreEqual(16, endOfInputPosition.Index);
        }

        [Test]
        public void TestAmbiguousSubtokens()
        {
            // item is a token and middle is a subtoken. middle is not atomic,
            // therefore it should run into the 2^N explosion and produce
            // multiple spans. however, becase the spans all start and end at
            // the same indexes, it should only result in one token.

            // setup
            string testGrammarText =
                "sequence = item+;\n" +
                "<token> item = 'start-' middle+ '-end' ;\n" +
                "<subtoken> middle = [\\l]+ ;\n";
            string testInputText = "start-ABCD-end";

            //

            var errors = new List<Error>();
            DefinitionExpression[] dis = (new SupergrammarSpanner()).GetExpressions(testGrammarText, errors);
            Assert.IsEmpty(errors);

            Grammar grammar = (new TokenizedGrammarBuilder()).BuildTokenizedGrammar(dis);
            Definition itemDef = grammar.FindDefinitionByName("item");
            Tokenizer tokenizer = new Tokenizer(grammar, testInputText);
            bool endOfInput;
            InputPosition endOfInputPosition;


            var tokens = tokenizer.GetTokensAtLocation(0, errors,
                                                       out endOfInput,
                                                       out endOfInputPosition);


            Assert.IsNotNull(errors);
            Assert.IsEmpty(errors);
            Assert.IsFalse(endOfInput);
            Assert.AreEqual(-1, endOfInputPosition.Index);
            Assert.IsNotNull(tokens);
            Assert.AreEqual(1, tokens.Count());
            var first = tokens.First();
            Assert.AreSame(itemDef, first.Definition);
            Assert.AreEqual(0, first.StartIndex);
            Assert.AreEqual("start-ABCD-end", first.Value);
        }
    }
}

