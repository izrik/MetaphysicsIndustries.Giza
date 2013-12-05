using System;
using System.Collections.Generic;
using System.Linq;
using MetaphysicsIndustries.Collections;

namespace MetaphysicsIndustries.Giza
{
    public class Spanner2 : ParserBase<CharacterSource, InputChar>
    {
        public Spanner2(Definition definition)
            : base(definition)
        {
        }

        public Span[] Process(CharacterSource input, ICollection<Error> errors)
        {
            return Parse(input, errors);
        }
        public NodeMatch<InputChar>[] Match(CharacterSource input, List<Error> errors, out bool endOfInput, out InputPosition endOfInputPosition, bool mustUseAllInput=true, int startIndex=0)
        {
            endOfInput = false;
            endOfInputPosition = new InputPosition();

            return Match(input, errors);
        }

        protected override bool IsBranchTip(NodeMatch<InputChar> cur)
        {
            return (cur.Node is CharNode);
        }

        #region implemented abstract members of ParserBase

        protected override bool BranchTipMatchesInputElement(NodeMatch<InputChar> branchTip, InputChar inputElement)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}

