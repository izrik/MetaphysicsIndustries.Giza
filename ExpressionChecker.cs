using System;
using System.Collections.Generic;

using System.Linq;
using System.Text;

namespace MetaphysicsIndustries.Giza
{
    public partial class ExpressionChecker
    {
        bool IsTokenized(DefinitionExpression def)
        {
            return
                def.Directives.Contains(DefinitionDirective.Token) ||
                def.Directives.Contains(DefinitionDirective.Subtoken) ||
                def.Directives.Contains(DefinitionDirective.Comment) ||
                def.Directives.Contains(DefinitionDirective.Whitespace);
        }

        public List<Error> CheckDefinitionInfosForParsing(IEnumerable<DefinitionExpression> defs)
        {
            var errors = CheckDefinitionInfos(defs);

            var defsByName = new Dictionary<string, DefinitionExpression>();
            foreach (var def in defs)
            {
                defsByName[def.Name] = def;
            }
            foreach (var def in defs)
            {
                int numTokenizedDirectives = 0;

                if (def.Directives.Contains(DefinitionDirective.Token)) numTokenizedDirectives++;
                if (def.Directives.Contains(DefinitionDirective.Subtoken)) numTokenizedDirectives++;
                if (def.Directives.Contains(DefinitionDirective.Comment)) numTokenizedDirectives++;
                if (def.Directives.Contains(DefinitionDirective.Whitespace)) numTokenizedDirectives++;

                if (numTokenizedDirectives > 1)
                {
                    errors.Add(new ExpressionError {
                        ErrorType = ExpressionError.MixedTokenizedDirectives,
                        DefinitionInfo = def,
                    });
                }

                if (!IsTokenized(def) &&
                    def.Directives.Contains(DefinitionDirective.Atomic))
                {
                    errors.Add(new ExpressionError {
                        ErrorType = ExpressionError.AtomicInNonTokenDefinition,
                        DefinitionInfo = def,
                    });
                }

                if (def.Directives.Contains(DefinitionDirective.Token) &&
                    def.Directives.Contains(DefinitionDirective.Atomic))
                {
                    errors.Add(new ExpressionError {
                        ErrorType = ExpressionError.AtomicInTokenDefinition,
                        DefinitionInfo = def,
                    });
                }

                if (def.Directives.Contains(DefinitionDirective.Comment) &&
                    def.Directives.Contains(DefinitionDirective.Atomic))
                {
                    errors.Add(new ExpressionError {
                        ErrorType = ExpressionError.AtomicInCommentDefinition,
                        DefinitionInfo = def,
                    });
                }

                if (!IsTokenized(def) &&
                    def.Directives.Contains(DefinitionDirective.MindWhitespace))
                {
                    errors.Add(new ExpressionError {
                        ErrorType = ExpressionError.MindWhitespaceInNonTokenDefinition,
                        DefinitionInfo = def,
                    });
                }

                if (IsTokenized(def) &&
                    def.Directives.Contains(DefinitionDirective.MindWhitespace))
                {
                    errors.Add(new ExpressionError {
                        ErrorType = ExpressionError.MindWhitespaceInTokenizedDefinition,
                        DefinitionInfo = def,
                    });
                }

                foreach (DefRefSubExpression defref in def.EnumerateDefRefs())
                {
                    if (!defsByName.ContainsKey(defref.DefinitionName)) continue;

                    DefinitionExpression target = defsByName[defref.DefinitionName];

                    if (def.Directives.Contains(DefinitionDirective.Whitespace))
                    {
                        errors.Add(new ExpressionError {
                            ErrorType = ExpressionError.WhitespaceReferencesDefinition,
                            ExpressionItem = defref,
                            DefinitionInfo = def,
                        });
                    }
                    else if (target.Directives.Contains(DefinitionDirective.Whitespace))
                    {
                        errors.Add(new ExpressionError {
                            ErrorType = ExpressionError.DefinitionReferencesWhitespace,
                            ExpressionItem = defref,
                            DefinitionInfo = def,
                        });
                    }
                    else
                    {
                        if (!IsTokenized(def) &&
                            target.Directives.Contains(DefinitionDirective.Subtoken))
                        {
                            errors.Add(new ExpressionError {
                                ErrorType = ExpressionError.NonTokenReferencesSubtoken,
                                ExpressionItem = defref,
                                DefinitionInfo = def,
                            });
                        }

                        if (!IsTokenized(def) &&
                            target.Directives.Contains(DefinitionDirective.Comment))
                        {
                            errors.Add(new ExpressionError {
                                ErrorType = ExpressionError.NonTokenReferencesComment,
                                ExpressionItem = defref,
                                DefinitionInfo = def,
                            });
                        }

                        if (IsTokenized(def) &&
                            target.Directives.Contains(DefinitionDirective.Token))
                        {
                            errors.Add(new ExpressionError {
                                ErrorType = ExpressionError.TokenizedReferencesToken,
                                ExpressionItem = defref,
                                DefinitionInfo = def,
                            });
                        }

                        if (IsTokenized(def) &&
                            target.Directives.Contains(DefinitionDirective.Comment))
                        {
                            errors.Add(new ExpressionError {
                                ErrorType = ExpressionError.TokenizedReferencesComment,
                                ExpressionItem = defref,
                                DefinitionInfo = def,
                            });
                        }

                        if (IsTokenized(def) &&
                            !IsTokenized(target))
                        {
                            errors.Add(new ExpressionError {
                                ErrorType = ExpressionError.TokenizedReferencesNonToken,
                                ExpressionItem = defref,
                                DefinitionInfo = def,
                            });
                        }
                    }
                }
            }

            if (defs.Count(def => def.Directives.Contains(DefinitionDirective.Whitespace)) > 1)
            {
                errors.Add(new ExpressionError {
                    ErrorType = ExpressionError.MultipleWhitespaceDefinitions,
                });
            }

            return errors;
        }

        public List<Error> CheckDefinitionInfosForSpanning(IEnumerable<DefinitionExpression> defs)
        {
            List<Error> errors = CheckDefinitionInfos(defs);

            var directives = new [] {
                DefinitionDirective.Token,
                DefinitionDirective.Subtoken,
                DefinitionDirective.Comment,
                DefinitionDirective.Whitespace,
            };

            foreach (var def in defs)
            {
                foreach (var directive in directives)
                {
                    if (def.Directives.Contains(directive))
                    {
                        errors.Add(new ExpressionError {
                            ErrorType = ExpressionError.TokenizedDirectiveInNonTokenizedGrammar,
                            DefinitionInfo = def,
                            DirectiveName = directive.GetName(),
                        });
                    }
                }
            }

            return errors;
        }

        public virtual List<Error> CheckDefinitionInfos(IEnumerable<DefinitionExpression> defs)
        {
            if (defs == null) throw new ArgumentNullException("defs");

            List<Error> errors = new List<Error>();

            // Any Expression or ExpressionItem object can be used in the 
            // trees only once. This precludes reference cycles as well as 
            // ADGs. ADGs are not much of a problem, but reference cycles 
            // would cause the checker to go into an infinite loop if we 
            // weren't expecting it.
            HashSet<Expression> visitedExprs = new HashSet<Expression>();
            HashSet<ExpressionItem> visitedItems = new HashSet<ExpressionItem>();
            HashSet<DefinitionExpression> visitedDefs = new HashSet<DefinitionExpression>();
            int index = -1;
            List<string> defNames = new List<string>();
            foreach (DefinitionExpression def in defs)
            {
                index++;
                if (def == null)
                {
                    errors.Add(new ExpressionError {
                        ErrorType=ExpressionError.NullDefinition,
                        Index=index,
                    });
                    continue;
                }

                defNames.Add(def.Name);
            }

            var defnames2 = new HashSet<string>();
            index = -1;
            foreach (DefinitionExpression def in defs)
            {
                index++;
                if (def == null) continue;

                if (visitedDefs.Contains(def))
                {
                    errors.Add(new ExpressionError {
                        ErrorType=ExpressionError.ReusedDefintion,
                        DefinitionInfo=def,
                        Index=index,
                    });
                    continue;
                }
                visitedDefs.Add(def);

                if (string.IsNullOrEmpty(def.Name))
                {
                    errors.Add(new ExpressionError {
                        ErrorType=ExpressionError.NullOrEmptyDefinitionName,
                        DefinitionInfo=def,
                        Index=index,
                    });
                }
                else if (defnames2.Contains(def.Name))
                {
                    errors.Add(new ExpressionError {
                        ErrorType=ExpressionError.DuplicateDefinitionName,
                        Index=index,
                        DefinitionInfo=def,
                    });
                }
                else
                {
                    defnames2.Add(def.Name);
                }

                CheckExpression(def, def, defNames, visitedExprs, visitedItems, errors);
            }

            return errors;
        }

        protected virtual void CheckExpression(DefinitionExpression def,
                             Expression expr,
                             List<string> defNames,
                             HashSet<Expression> visitedExprs,
                             HashSet<ExpressionItem> visitedItems,
                             List<Error> errors)
        {
            if (visitedExprs.Contains(expr))
            {
                errors.Add(new ExpressionError {
                    ErrorType=ExpressionError.ReusedExpression,
                    Expression=expr,
                    DefinitionInfo=def,
                });
                return;
            }
            visitedExprs.Add(expr);

            if (expr.Items == null || expr.Items.Count < 1)
            {
                errors.Add(new ExpressionError {
                    ErrorType = ExpressionError.EmptyExpressionItems,
                    Expression = expr,
                    DefinitionInfo = def,
                });
            }
            else
            {
                bool skippable = true;

                int index = 0;
                foreach (ExpressionItem item in expr.Items)
                {
                    if (item == null)
                    {
                        errors.Add(new ExpressionError {
                            ErrorType = ExpressionError.NullExpressionItem,
                            Expression = expr,
                            Index = index,
                            DefinitionInfo = def
                        });
                    }
                    else
                    {
                        CheckExpressionItem(def, item, defNames, visitedExprs, visitedItems, errors);
                        skippable = skippable && item.IsSkippable;
                    }

                    index++;
                }

                if (skippable && expr.Items.Count > 0)
                {
                    errors.Add(new ExpressionError {
                        ErrorType = ExpressionError.AllItemsSkippable,
                        Expression = expr,
                        DefinitionInfo = def,
                    });
                }
            }
        }

        protected virtual void CheckExpressionItem(DefinitionExpression def,
                                 ExpressionItem item,
                                 List<string> defNames,
                                 HashSet<Expression> visitedExprs,
                                 HashSet<ExpressionItem> visitedItems,
                                 List<Error> errors)
        {
            if (item == null) throw new ArgumentNullException("item");

            if (visitedItems.Contains(item))
            {
                errors.Add(new ExpressionError {
                    ErrorType=ExpressionError.ReusedExpressionItem,
                    ExpressionItem=item,
                    DefinitionInfo=def,
                });
                return;
            }
            visitedItems.Add(item);

            if (item is OrExpression)
            {
                OrExpression orexor = (OrExpression)item;
                if (orexor.Expressions.Count < 1)
                {
                    errors.Add(new ExpressionError {
                        ErrorType = ExpressionError.EmptyOrexprExpressionList,
                        DefinitionInfo = def,
                        ExpressionItem = item
                    });
                }
                else
                {
                    int index = 0;
                    foreach (Expression expr in orexor.Expressions)
                    {
                        if (expr == null)
                        {
                            errors.Add(new ExpressionError {
                                ErrorType = ExpressionError.NullOrexprExpression,
                                ExpressionItem = item,
                                Index = index,
                                DefinitionInfo = def,
                            });
                        }
                        else
                        {
                            CheckExpression(def, expr, defNames, visitedExprs, visitedItems, errors);
                        }
                        index++;
                    }
                }
            }
            else if (item is SubExpression)
            {
                if ((item as SubExpression).Tag == null)
                {
                    errors.Add(new ExpressionError {
                        ErrorType = ExpressionError.NullSubexprTag,
                        ExpressionItem = item,
                        DefinitionInfo = def,
                    });
                }

                if (item is LiteralSubExpression)
                {
                    if (string.IsNullOrEmpty((item as LiteralSubExpression).Value))
                    {
                        errors.Add(new ExpressionError {
                            ErrorType = ExpressionError.NullOrEmptyLiteralValue,
                            ExpressionItem = item,
                            DefinitionInfo = def,
                        });
                    }
                }
                else if (item is CharClassSubExpression)
                {
                    CharClass cc = (item as CharClassSubExpression).CharClass;

                    if (cc == null || cc.GetAllCharsCount() < 1)
                    {
                        errors.Add(new ExpressionError {
                            ErrorType = ExpressionError.NullOrEmptyCharClass,
                            ExpressionItem = item,
                            DefinitionInfo = def
                        });
                    }
                }
                else if (item is DefRefSubExpression)
                {
                    string name = (item as DefRefSubExpression).DefinitionName;
                    if (string.IsNullOrEmpty(name))
                    {
                        errors.Add(new ExpressionError {
                            ErrorType = ExpressionError.NullOrEmptyDefrefName,
                            ExpressionItem = item,
                            DefinitionInfo = def,
                        });
                    }
                    else if (!defNames.Contains(name))
                    {
                        errors.Add(new ExpressionError {
                            ErrorType = ExpressionError.DefRefNameNotFound,
                            ExpressionItem = item,
                            DefinitionInfo = def,
                        });
                    }
                }
                else
                {
                    throw new InvalidOperationException(
                        string.Format(
                        "Unknown SubExpression sub-type: {0}", 
                        item.GetType()));
                }
            }
            else
            {
                throw new InvalidOperationException(
                    string.Format(
                        "Unknown ExpressionItem sub-type: {0}", 
                        item.GetType()));
            }
        }

        //Errors for both tokenized and non-tokenize grammars
        //
        // E0  DefinitionInfo[] array is null (argnull exception)
        // T1  Any Expression or ExpressionItem re-use (of which reference cycles are a subset)
        // T2  DefinitionInfo.Name is null or empty
        // T3  DefinitionInfo.Directives is null
        // T4  DefinitionInfo.Expression is null
        // T6  Expression.Items.Length less than one (empty)
        // T7  OrExpression.Expressions is empty
        // T8  SubExpression.Tag is null
        // T9  DefRefSubExpression.DefinitionName is null or empty
        // T10 LiteralSubExpression.Value is null or empty
        // T11 CharClassSubExpression.CharClass is null
        // T12 CharClassSubExpression.CharClass doesn't cover any characters or patterns
        // S2  More than one DefinitionInfo has a given Name
        // S3  All items in the expression are skippable
        // S4  Any expressions in the orexpr is skippable
        // S5  A defref's DefinitionName is not found in the containing DefinitionInfo[] array
        // S?  Leading reference cycle?

        //Errors for tokenized grammars
        //
        // S7  A non-token def references a subtoken def
        // S8  A subtoken def refs a non-token ref
        // S9  A token def refs a non-token def
        // S10 A subtoken def refs a token def
        // S11 A definition has both token and subtoken directives
        // S12 A definition has both comment and subtoken directives
        // S13 Any def refs a comment def
    }
}

