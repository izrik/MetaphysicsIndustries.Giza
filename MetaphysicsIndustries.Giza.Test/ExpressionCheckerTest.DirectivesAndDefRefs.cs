using System;
using System.Collections.Generic;
using NUnit.Framework;
using System.Linq;

// this/that --> this references that
// token/token              n
// token/subtoken           y
// token/comment            n
// token/whitspace          n
// token/nontoken           n
// subtoken/token           n
// subtoken/subtoken        y
// subtoken/comment         n
// subtoken/nontoken        n
// subtoken/whitespace      n
// comment/token            n
// comment/subtoken         y
// comment/comment          n
// comment/nontoken         n
// comment/whitespace       n
// whitespace/token         n
// whitespace/subtoken      n
// whitespace/comment       n
// whitespace/nontoken      n
// whitespace/whitespace    n
// nontoken/token           y
// nontoken/subtoken        n
// nontoken/comment         n
// nontoken/nontoken        y
// nontoken/whitespace      n

namespace MetaphysicsIndustries.Giza.Test
{
    public partial class ExpressionCheckerTest
    {
        [Test]
        public void TestTokenReferencesToken()
        {
            DefinitionExpression[] defs = {
                new DefinitionExpression {
                    Name = "A",
                },
                new DefinitionExpression {
                    Name = "B",
                },
            };
            defs[0].Directives.Add(DefinitionDirective.Token);
            defs[0].Items.Add(new LiteralSubExpression { Value = "literal" });
            defs[1].Directives.Add(DefinitionDirective.Token);
            defs[1].Items.Add(new DefRefSubExpression { DefinitionName = "A" });

            ExpressionChecker ec = new ExpressionChecker();
            List<Error> errors = ec.CheckDefinitionInfosForParsing(defs);

            Assert.IsNotNull(errors);
            Assert.AreEqual(1, errors.Count);
            Assert.AreEqual(ExpressionError.TokenizedReferencesToken, errors[0].ErrorType);
            Assert.IsInstanceOf<ExpressionError>(errors[0]);
            var err = (errors[0] as ExpressionError);
            Assert.AreEqual(null, err.Expression);
            Assert.AreSame(defs[1].Items[0], err.ExpressionItem);
            Assert.AreSame(defs[1], err.DefinitionInfo);
        }

        [Test]
        public void TestTokenReferencesSubtoken()
        {
            DefinitionExpression[] defs = {
                new DefinitionExpression {
                    Name = "A",
                },
                new DefinitionExpression {
                    Name = "B",
                },
            };
            defs[0].Directives.Add(DefinitionDirective.Subtoken);
            defs[0].Items.Add(new LiteralSubExpression { Value = "literal" });
            defs[1].Directives.Add(DefinitionDirective.Token);
            defs[1].Items.Add(new DefRefSubExpression { DefinitionName = "A" });

            ExpressionChecker ec = new ExpressionChecker();
            List<Error> errors = ec.CheckDefinitionInfosForParsing(defs);

            Assert.IsNotNull(errors);
            Assert.AreEqual(0, errors.Count);
        }

        [Test]
        public void TestTokenReferencesComment()
        {
            DefinitionExpression[] defs = {
                new DefinitionExpression {
                    Name = "A",
                },
                new DefinitionExpression {
                    Name = "B",
                },
            };
            defs[0].Directives.Add(DefinitionDirective.Comment);
            defs[0].Items.Add(new LiteralSubExpression { Value = "literal" });
            defs[1].Directives.Add(DefinitionDirective.Token);
            defs[1].Items.Add(new DefRefSubExpression { DefinitionName = "A" });

            ExpressionChecker ec = new ExpressionChecker();
            List<Error> errors = ec.CheckDefinitionInfosForParsing(defs);

            Assert.IsNotNull(errors);
            Assert.AreEqual(1, errors.Count);
            Assert.AreEqual(ExpressionError.TokenizedReferencesComment, errors[0].ErrorType);
            Assert.IsInstanceOf<ExpressionError>(errors[0]);
            var err = (errors[0] as ExpressionError);
            Assert.AreEqual(null, err.Expression);
            Assert.AreSame(defs[1].Items[0], err.ExpressionItem);
            Assert.AreSame(defs[1], err.DefinitionInfo);
        }

        [Test]
        public void TestTokenReferencesNonToken()
        {
            DefinitionExpression[] defs = {
                new DefinitionExpression {
                    Name = "A",
                },
                new DefinitionExpression {
                    Name = "B",
                },
            };
            defs[0].Items.Add(new LiteralSubExpression { Value = "literal" });
            defs[1].Directives.Add(DefinitionDirective.Token);
            defs[1].Items.Add(new DefRefSubExpression { DefinitionName = "A" });

            ExpressionChecker ec = new ExpressionChecker();
            List<Error> errors = ec.CheckDefinitionInfosForParsing(defs);

            Assert.IsNotNull(errors);
            Assert.AreEqual(1, errors.Count);
            Assert.AreEqual(ExpressionError.TokenizedReferencesNonToken, errors[0].ErrorType);
            Assert.IsInstanceOf<ExpressionError>(errors[0]);
            var err = (errors[0] as ExpressionError);
            Assert.AreEqual(null, err.Expression);
            Assert.AreSame(defs[1].Items[0], err.ExpressionItem);
            Assert.AreSame(defs[1], err.DefinitionInfo);
        }

        [Test]
        public void TestSubtokenReferencesToken()
        {
            DefinitionExpression[] defs = {
                new DefinitionExpression {
                    Name = "A",
                },
                new DefinitionExpression {
                    Name = "B",
                },
            };
            defs[0].Directives.Add(DefinitionDirective.Token);
            defs[0].Items.Add(new LiteralSubExpression { Value = "literal" });
            defs[1].Directives.Add(DefinitionDirective.Subtoken);
            defs[1].Items.Add(new DefRefSubExpression { DefinitionName = "A" });

            ExpressionChecker ec = new ExpressionChecker();
            List<Error> errors = ec.CheckDefinitionInfosForParsing(defs);

            Assert.IsNotNull(errors);
            Assert.AreEqual(1, errors.Count);
            Assert.AreEqual(ExpressionError.TokenizedReferencesToken, errors[0].ErrorType);
            Assert.IsInstanceOf<ExpressionError>(errors[0]);
            var err = (errors[0] as ExpressionError);
            Assert.AreEqual(null, err.Expression);
            Assert.AreSame(defs[1].Items[0], err.ExpressionItem);
            Assert.AreSame(defs[1], err.DefinitionInfo);
        }

        [Test]
        public void TestSubtokenReferencesSubtoken()
        {
            DefinitionExpression[] defs = {
                new DefinitionExpression {
                    Name = "A",
                },
                new DefinitionExpression {
                    Name = "B",
                },
            };
            defs[0].Directives.Add(DefinitionDirective.Subtoken);
            defs[0].Items.Add(new LiteralSubExpression { Value = "literal" });
            defs[1].Directives.Add(DefinitionDirective.Subtoken);
            defs[1].Items.Add(new DefRefSubExpression { DefinitionName = "A" });

            ExpressionChecker ec = new ExpressionChecker();
            List<Error> errors = ec.CheckDefinitionInfosForParsing(defs);

            Assert.IsNotNull(errors);
            Assert.AreEqual(0, errors.Count);
        }

        [Test]
        public void TestSubtokenReferencesComment()
        {
            DefinitionExpression[] defs = {
                new DefinitionExpression {
                    Name = "A",
                },
                new DefinitionExpression {
                    Name = "B",
                },
            };
            defs[0].Directives.Add(DefinitionDirective.Comment);
            defs[0].Items.Add(new LiteralSubExpression { Value = "literal" });
            defs[1].Directives.Add(DefinitionDirective.Subtoken);
            defs[1].Items.Add(new DefRefSubExpression { DefinitionName = "A" });

            ExpressionChecker ec = new ExpressionChecker();
            List<Error> errors = ec.CheckDefinitionInfosForParsing(defs);

            Assert.IsNotNull(errors);
            Assert.AreEqual(1, errors.Count);
            Assert.AreEqual(ExpressionError.TokenizedReferencesComment, errors[0].ErrorType);
            Assert.IsInstanceOf<ExpressionError>(errors[0]);
            var err = (errors[0] as ExpressionError);
            Assert.AreEqual(null, err.Expression);
            Assert.AreSame(defs[1].Items[0], err.ExpressionItem);
            Assert.AreSame(defs[1], err.DefinitionInfo);
        }

        [Test()]
        public void TestSubtokenReferencesNonToken()
        {
            DefinitionExpression[] defs = {
                new DefinitionExpression {
                    Name = "A",
                },
                new DefinitionExpression {
                    Name = "B",
                },
            };
            defs[0].Items.Add(new LiteralSubExpression { Value = "literal" });
            defs[1].Directives.Add(DefinitionDirective.Subtoken);
            defs[1].Items.Add(new DefRefSubExpression { DefinitionName = "A" });

            ExpressionChecker ec = new ExpressionChecker();
            List<Error> errors = ec.CheckDefinitionInfosForParsing(defs);

            Assert.IsNotNull(errors);
            Assert.AreEqual(1, errors.Count);
            Assert.AreEqual(ExpressionError.TokenizedReferencesNonToken, errors[0].ErrorType);
            Assert.IsInstanceOf<ExpressionError>(errors[0]);
            var err = (errors[0] as ExpressionError);
            Assert.AreEqual(null, err.Expression);
            Assert.AreSame(defs[1].Items[0], err.ExpressionItem);
            Assert.AreSame(defs[1], err.DefinitionInfo);
        }

        [Test]
        public void TestCommentReferencesToken()
        {
            DefinitionExpression[] defs = {
                new DefinitionExpression {
                    Name = "A",
                },
                new DefinitionExpression {
                    Name = "B",
                },
            };
            defs[0].Directives.Add(DefinitionDirective.Token);
            defs[0].Items.Add(new LiteralSubExpression { Value = "literal" });
            defs[1].Directives.Add(DefinitionDirective.Comment);
            defs[1].Items.Add(new DefRefSubExpression { DefinitionName = "A" });

            ExpressionChecker ec = new ExpressionChecker();
            List<Error> errors = ec.CheckDefinitionInfosForParsing(defs);

            Assert.IsNotNull(errors);
            Assert.AreEqual(1, errors.Count);
            Assert.AreEqual(ExpressionError.TokenizedReferencesToken, errors[0].ErrorType);
            Assert.IsInstanceOf<ExpressionError>(errors[0]);
            var err = (errors[0] as ExpressionError);
            Assert.AreEqual(null, err.Expression);
            Assert.AreSame(defs[1].Items[0], err.ExpressionItem);
            Assert.AreSame(defs[1], err.DefinitionInfo);
        }

        [Test]
        public void TestCommentReferencesSubtoken()
        {
            DefinitionExpression[] defs = {
                new DefinitionExpression {
                    Name = "A",
                },
                new DefinitionExpression {
                    Name = "B",
                },
            };
            defs[0].Directives.Add(DefinitionDirective.Subtoken);
            defs[0].Items.Add(new LiteralSubExpression { Value = "literal" });
            defs[1].Directives.Add(DefinitionDirective.Comment);
            defs[1].Items.Add(new DefRefSubExpression { DefinitionName = "A" });

            ExpressionChecker ec = new ExpressionChecker();
            List<Error> errors = ec.CheckDefinitionInfosForParsing(defs);

            Assert.IsNotNull(errors);
            Assert.AreEqual(0, errors.Count);
        }

        [Test]
        public void TestCommentReferencesComment()
        {
            DefinitionExpression[] defs = {
                new DefinitionExpression {
                    Name = "A",
                },
                new DefinitionExpression {
                    Name = "B",
                },
            };
            defs[0].Directives.Add(DefinitionDirective.Comment);
            defs[0].Items.Add(new LiteralSubExpression { Value = "literal" });
            defs[1].Directives.Add(DefinitionDirective.Comment);
            defs[1].Items.Add(new DefRefSubExpression { DefinitionName = "A" });

            ExpressionChecker ec = new ExpressionChecker();
            List<Error> errors = ec.CheckDefinitionInfosForParsing(defs);

            Assert.IsNotNull(errors);
            Assert.AreEqual(1, errors.Count);
            Assert.AreEqual(ExpressionError.TokenizedReferencesComment, errors[0].ErrorType);
            Assert.IsInstanceOf<ExpressionError>(errors[0]);
            var err = (errors[0] as ExpressionError);
            Assert.AreEqual(null, err.Expression);
            Assert.AreSame(defs[1].Items[0], err.ExpressionItem);
            Assert.AreSame(defs[1], err.DefinitionInfo);
        }

        [Test]
        public void TestCommentReferencesNonToken()
        {
            DefinitionExpression[] defs = {
                new DefinitionExpression {
                    Name = "A",
                },
                new DefinitionExpression {
                    Name = "B",
                },
            };
            defs[0].Items.Add(new LiteralSubExpression { Value = "literal" });
            defs[1].Directives.Add(DefinitionDirective.Comment);
            defs[1].Items.Add(new DefRefSubExpression { DefinitionName = "A" });

            ExpressionChecker ec = new ExpressionChecker();
            List<Error> errors = ec.CheckDefinitionInfosForParsing(defs);

            Assert.IsNotNull(errors);
            Assert.AreEqual(1, errors.Count);
            Assert.AreEqual(ExpressionError.TokenizedReferencesNonToken, errors[0].ErrorType);
            Assert.IsInstanceOf<ExpressionError>(errors[0]);
            var err = (errors[0] as ExpressionError);
            Assert.AreEqual(null, err.Expression);
            Assert.AreSame(defs[1].Items[0], err.ExpressionItem);
            Assert.AreSame(defs[1], err.DefinitionInfo);
        }

        [Test]
        public void TestNonTokenReferencesToken()
        {
            DefinitionExpression[] defs = {
                new DefinitionExpression {
                    Name = "A",
                },
                new DefinitionExpression {
                    Name = "B",
                },
            };
            defs[0].Directives.Add(DefinitionDirective.Token);
            defs[0].Items.Add(new LiteralSubExpression { Value = "literal" });
            defs[1].Items.Add(new DefRefSubExpression { DefinitionName = "A" });

            ExpressionChecker ec = new ExpressionChecker();
            List<Error> errors = ec.CheckDefinitionInfosForParsing(defs);

            Assert.IsNotNull(errors);
            Assert.AreEqual(0, errors.Count);
        }

        [Test]
        public void TestNonTokenReferencesSubtoken()
        {
            DefinitionExpression[] defs = {
                new DefinitionExpression {
                    Name = "A",
                },
                new DefinitionExpression {
                    Name = "B",
                },
            };
            defs[0].Directives.Add(DefinitionDirective.Subtoken);
            defs[0].Items.Add(new LiteralSubExpression { Value = "literal" });
            defs[1].Items.Add(new DefRefSubExpression { DefinitionName = "A" });

            ExpressionChecker ec = new ExpressionChecker();
            List<Error> errors = ec.CheckDefinitionInfosForParsing(defs);

            Assert.IsNotNull(errors);
            Assert.AreEqual(1, errors.Count);
            Assert.AreEqual(ExpressionError.NonTokenReferencesSubtoken, errors[0].ErrorType);
            Assert.IsInstanceOf<ExpressionError>(errors[0]);
            var err = (errors[0] as ExpressionError);
            Assert.AreEqual(null, err.Expression);
            Assert.AreSame(defs[1].Items[0], err.ExpressionItem);
            Assert.AreSame(defs[1], err.DefinitionInfo);
        }

        [Test()]
        public void TestNonTokenReferencesComment()
        {
            DefinitionExpression[] defs = {
                new DefinitionExpression {
                    Name = "A",
                },
                new DefinitionExpression {
                    Name = "B",
                },
            };
            defs[0].Directives.Add(DefinitionDirective.Comment);
            defs[0].Items.Add(new LiteralSubExpression { Value = "literal" });
            defs[1].Items.Add(new DefRefSubExpression { DefinitionName = "A" });

            ExpressionChecker ec = new ExpressionChecker();
            List<Error> errors = ec.CheckDefinitionInfosForParsing(defs);

            Assert.IsNotNull(errors);
            Assert.AreEqual(1, errors.Count);
            Assert.AreEqual(ExpressionError.NonTokenReferencesComment, errors[0].ErrorType);
            Assert.IsInstanceOf<ExpressionError>(errors[0]);
            var err = (errors[0] as ExpressionError);
            Assert.AreEqual(null, err.Expression);
            Assert.AreSame(defs[1].Items[0], err.ExpressionItem);
            Assert.AreSame(defs[1], err.DefinitionInfo);
        }

        [Test]
        public void TestNonTokenReferencesNonToken()
        {
            DefinitionExpression[] defs = {
                new DefinitionExpression {
                    Name = "A",
                },
                new DefinitionExpression {
                    Name = "B",
                },
            };
            defs[0].Items.Add(new LiteralSubExpression { Value = "literal" });
            defs[1].Items.Add(new DefRefSubExpression { DefinitionName = "A" });

            ExpressionChecker ec = new ExpressionChecker();
            List<Error> errors = ec.CheckDefinitionInfosForParsing(defs);

            Assert.IsNotNull(errors);
            Assert.AreEqual(0, errors.Count);
        }

        [Test]
        [TestCase(DefinitionDirective.Token)]
        [TestCase(DefinitionDirective.Subtoken)]
        [TestCase(DefinitionDirective.Comment)]
        public void TestTokenizedReferencesWhitespace(DefinitionDirective directive)
        {
            DefinitionExpression[] defs = {
                new DefinitionExpression {
                    Name = "A",
                },
                new DefinitionExpression {
                    Name = "B",
                },
            };
            defs[0].Directives.Add(DefinitionDirective.Whitespace);
            defs[0].Items.Add(new LiteralSubExpression { Value = "literal" });
            defs[1].Directives.Add(directive);
            defs[1].Items.Add(new DefRefSubExpression { DefinitionName = "A" });

            var ec = new ExpressionChecker();
            var errors = ec.CheckDefinitionInfosForParsing(defs);

            Assert.IsNotNull(errors);
            Assert.AreEqual(1, errors.Count);
            Assert.AreEqual(ExpressionError.DefinitionReferencesWhitespace, errors[0].ErrorType);
            Assert.IsInstanceOf<ExpressionError>(errors[0]);
            var err = (errors[0] as ExpressionError);
            Assert.AreEqual(null, err.Expression);
            Assert.AreSame(defs[1].Items[0], err.ExpressionItem);
            Assert.AreSame(defs[1], err.DefinitionInfo);
        }


        [Test()]
        public void TestNonTokenReferencesWhitespace()
        {
            DefinitionExpression[] defs = {
                new DefinitionExpression {
                    Name = "A",
                },
                new DefinitionExpression {
                    Name = "B",
                },
            };
            defs[0].Directives.Add(DefinitionDirective.Whitespace);
            defs[0].Items.Add(new LiteralSubExpression { Value = "literal" });
            defs[1].Items.Add(new DefRefSubExpression { DefinitionName = "A" });

            var ec = new ExpressionChecker();
            List<Error> errors = ec.CheckDefinitionInfosForParsing(defs);

            Assert.IsNotNull(errors);
            Assert.AreEqual(1, errors.Count);
            Assert.AreEqual(ExpressionError.DefinitionReferencesWhitespace, errors[0].ErrorType);
            Assert.IsInstanceOf<ExpressionError>(errors[0]);
            var err = (errors[0] as ExpressionError);
            Assert.AreEqual(null, err.Expression);
            Assert.AreSame(defs[1].Items[0], err.ExpressionItem);
            Assert.AreSame(defs[1], err.DefinitionInfo);
        }

        [Test]
        [TestCase(DefinitionDirective.Token)]
        [TestCase(DefinitionDirective.Subtoken)]
        [TestCase(DefinitionDirective.Comment)]
        public void TestWhitespaceReferencesTokenized(DefinitionDirective directive)
        {
            DefinitionExpression[] defs = {
                new DefinitionExpression {
                    Name = "A",
                },
                new DefinitionExpression {
                    Name = "B",
                },
            };
            defs[0].Directives.Add(directive);
            defs[0].Items.Add(new LiteralSubExpression { Value = "literal" });
            defs[1].Directives.Add(DefinitionDirective.Whitespace);
            defs[1].Items.Add(new DefRefSubExpression { DefinitionName = "A" });

            var ec = new ExpressionChecker();
            List<Error> errors = ec.CheckDefinitionInfosForParsing(defs);

            Assert.IsNotNull(errors);
            Assert.AreEqual(1, errors.Count);
            Assert.AreEqual(ExpressionError.WhitespaceReferencesDefinition, errors[0].ErrorType);
            Assert.IsInstanceOf<ExpressionError>(errors[0]);
            var err = (errors[0] as ExpressionError);
            Assert.AreEqual(null, err.Expression);
            Assert.AreSame(defs[1].Items[0], err.ExpressionItem);
            Assert.AreSame(defs[1], err.DefinitionInfo);
        }

        [Test]
        public void TestWhitespaceReferencesNonTokenized()
        {
            DefinitionExpression[] defs = {
                new DefinitionExpression {
                    Name = "A",
                },
                new DefinitionExpression {
                    Name = "B",
                },
            };
            defs[0].Items.Add(new LiteralSubExpression { Value = "literal" });
            defs[1].Directives.Add(DefinitionDirective.Whitespace);
            defs[1].Items.Add(new DefRefSubExpression { DefinitionName = "A" });

            var ec = new ExpressionChecker();
            List<Error> errors = ec.CheckDefinitionInfosForParsing(defs);

            Assert.IsNotNull(errors);
            Assert.AreEqual(1, errors.Count);
            Assert.AreEqual(ExpressionError.WhitespaceReferencesDefinition, errors[0].ErrorType);
            Assert.IsInstanceOf<ExpressionError>(errors[0]);
            var err = (errors[0] as ExpressionError);
            Assert.AreEqual(null, err.Expression);
            Assert.AreSame(defs[1].Items[0], err.ExpressionItem);
            Assert.AreSame(defs[1], err.DefinitionInfo);
        }

        [Test]
        public void TestWhitespaceReferencesSelf()
        {
            DefinitionExpression[] defs = {
                new DefinitionExpression {
                    Name = "A",
                },
            };
            defs[0].Directives.Add(DefinitionDirective.Whitespace);
            defs[0].Items.Add(new LiteralSubExpression { Value = "literal" });
            defs[0].Items.Add(new DefRefSubExpression { DefinitionName = "A" });

            var ec = new ExpressionChecker();
            List<Error> errors = ec.CheckDefinitionInfosForParsing(defs);

            Assert.IsNotNull(errors);
            Assert.AreEqual(1, errors.Count);
            Assert.AreEqual(ExpressionError.WhitespaceReferencesDefinition, errors[0].ErrorType);
            Assert.IsInstanceOf<ExpressionError>(errors[0]);
            var err = (errors[0] as ExpressionError);
            Assert.AreEqual(null, err.Expression);
            Assert.AreSame(defs[0].Items[1], err.ExpressionItem);
            Assert.AreSame(defs[0], err.DefinitionInfo);
        }

        [Test]
        public void TestMultipleWhitespaceDefinitions()
        {
            DefinitionExpression[] defs = {
                new DefinitionExpression {
                    Name = "A",
                },
                new DefinitionExpression {
                    Name = "B",
                },
            };
            defs[0].Directives.Add(DefinitionDirective.Whitespace);
            defs[0].Items.Add(new LiteralSubExpression { Value = "literal" });
            defs[1].Directives.Add(DefinitionDirective.Whitespace);
            defs[1].Items.Add(new LiteralSubExpression { Value = "literal" });

            var ec = new ExpressionChecker();
            List<Error> errors = ec.CheckDefinitionInfosForParsing(defs);

            Assert.IsNotNull(errors);
            Assert.AreEqual(1, errors.Count);
            Assert.AreEqual(ExpressionError.MultipleWhitespaceDefinitions, errors[0].ErrorType);
            Assert.IsInstanceOf<ExpressionError>(errors[0]);
            var err = (errors[0] as ExpressionError);
            Assert.AreEqual(null, err.Expression);
            Assert.AreEqual(null, err.ExpressionItem);
            Assert.AreEqual(null, err.DefinitionInfo);
        }

    }
}

