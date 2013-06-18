using System;
using System.Collections.Generic;

namespace MetaphysicsIndustries.Giza
{
    public class Tokenizer
    {
        public Tokenizer(Grammar grammar)
        {
            _grammar = grammar.Clone();
            _tokenDef = new Definition("$token");
            _grammar.Definitions.Add(_tokenDef);

            foreach (Definition def in grammar.Definitions)
            {
                if (def.Directives.Contains(DefinitionDirective.Token))
                {
                    DefRefNode node = new DefRefNode(def);
                    _tokenDef.Nodes.Add(node);
                    _tokenDef.StartNodes.Add(node);
                    _tokenDef.EndNodes.Add(node);
                }
            }
        }

        Grammar _grammar;
        Spanner _spanner = new Spanner();
        Definition _tokenDef;

        public Token[] GetTokensAtLocation(string input, int index, out string error)
        {
            Spanner.NodeMatch[] matchTreeLeaves = _spanner.Match(_tokenDef, input, out error, false, index);

            List<Token> tokens = new List<Token>();
            foreach (Spanner.NodeMatch leaf in matchTreeLeaves)
            {
                Spanner.NodeMatch tokenEnd = leaf.Previous;
                Spanner.NodeMatch tokenStart = leaf.Previous.StartDef;

                tokens.Add(new Token{
                    Definition = tokenEnd.Previous.Node.ParentDefinition,
                    StartIndex = tokenStart.Index,
                    Length = leaf.Index - tokenStart.Index + 1
                });
            }

            return tokens.ToArray();
        }
    }
}
