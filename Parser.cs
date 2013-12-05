using System;
using System.Collections.Generic;
using MetaphysicsIndustries.Collections;
using System.Linq;
using System.Text;

namespace MetaphysicsIndustries.Giza
{
    public class Parser : ParserBase<ITokenSource, Token>
    {
        public Parser(Definition definition)
            : base(definition)
        {
        }

        public Span[] Parse(CharacterSource input, ICollection<Error> errors)
        {
            if (input == null) throw new ArgumentNullException("input");
            if (errors == null) throw new ArgumentNullException("errors");

            ITokenSource tokenSource = new Tokenizer(_definition.ParentGrammar, input);

            return Parse(tokenSource, errors);
        }
        public NodeMatch<Token>[] Match(CharacterSource input, ICollection<Error> errors)
        {
            if (input == null) throw new ArgumentNullException("input");
            if (errors == null) throw new ArgumentNullException("errors");

            ITokenSource tokenSource = new Tokenizer(_definition.ParentGrammar, input);

            return Match(tokenSource, errors);
        }
        protected override bool BranchTipMatchesInputElement(NodeMatch<Token> branchTip, Token inputElement)
        {
            return (branchTip.Node is DefRefNode) &&
                (branchTip.Node as DefRefNode).DefRef == inputElement.Definition;
        }
    }

    public abstract class ParserBase<TSource, TElement>
        where TSource : IInputSource<TElement>
        where TElement : IInputElement
    {
//        public class NodeMatch : NodeMatch<TElement>
//        {
//            public NodeMatch(Node node, TransitionType transition, NodeMatch<TElement> previous)
//                : base(node, transition, previous)
//            {
//            }
//        }

        public ParserBase(Definition definition)
        {
            if (definition == null) throw new ArgumentNullException("definition");

            _definition = definition;
        }

        protected readonly Definition _definition;

        class ParseInfo
        {
            public NodeMatchStackPair<TElement> SourcePair;
            public NodeMatch<TElement> Source { get { return SourcePair.NodeMatch; } }
            public MatchStack<TElement> SourceStack { get { return SourcePair.MatchStack; } }

            public NodeMatch<TElement> EndCandidate;

            public List<NodeMatchStackPair<TElement>> Branches;

            public IEnumerable<Node> GetExpectedNodes()
            {
                if (this.Source.Node.NextNodes.Count > 0)
                {
                    return this.Source.Node.NextNodes;
                }

                var stack = this.SourceStack;
                while (stack != null &&
                       stack.Node.NextNodes.Count < 1)
                {
                    stack = stack.Parent;
                }

                if (stack != null)
                {
                    return stack.Node.NextNodes;
                }

                return new Node[0];
            }
        }

        public Span[] Parse(TSource tokenSource, ICollection<Error> errors)
        {
            if (tokenSource == null) throw new ArgumentNullException("tokenSource");
            if (errors == null) throw new ArgumentNullException("errors");

            var matchTreeLeaves = Match(tokenSource, errors);

            return MakeSpans(matchTreeLeaves);
        }
        public NodeMatch<TElement>[] Match(TSource tokenSource, ICollection<Error> errors)
        {
            if (tokenSource == null) throw new ArgumentNullException("tokenSource");
            if (errors == null) throw new ArgumentNullException("errors");

            var sources = new PriorityQueue<NodeMatchStackPair<TElement>, int>(lowToHigh: true);
            var ends = new List<NodeMatch<TElement>>();
            var rootDef = new Definition("$rootDef");
            var rootNode = new DefRefNode(_definition, "$rootNode");
            rootDef.Nodes.Add(rootNode);
            rootDef.StartNodes.Add(rootNode);
            rootDef.EndNodes.Add(rootNode);
            var root = new NodeMatch<TElement>(rootNode, NodeMatch<TElement>.TransitionType.Root, null);
            var rejects = new List<NodeMatchErrorPair<TElement>>();

            var branches2 = new PriorityQueue<Tuple<NodeMatch<TElement>, MatchStack<TElement>, ParseInfo>, int>();

            sources.Enqueue(pair(root, null), -1);
//            Logger.WriteLine("Starting");

            while (sources.Count > 0)
            {

                var nextSources = new List<NodeMatchStackPair<TElement>>();
                while (sources.Count > 0)
                {
                    var sourcepair = sources.Dequeue();

                    var info = GetParseInfoFromSource(sourcepair);

                    if (info.EndCandidate != null)
                    {
                        ends.Add(info.EndCandidate);
                    }

                    //get all tokens, starting at end of source's token
                    var tokenization = tokenSource.GetInputAtLocation(info.Source.Token.IndexOfNextTokenization);

                    //if we get any tokenization errors, process them and reject
                    if (tokenization.Errors.ContainsNonWarnings())
                    {
                        //reject branches with errors

                        foreach (var branch in info.Branches)
                        {
                            rejects.Add(branch.NodeMatch, tokenization.Errors);
                        }

                        RejectEndCandidate(info, rejects, ends, tokenization.Errors);
                    }
                    else if (tokenization.EndOfInput)
                    {
                        var err = new ParserError {
                            ErrorType = ParserError.UnexpectedEndOfInput,
                            LastValidMatchingNode = info.Source.Node,
                            ExpectedNodes = info.GetExpectedNodes(),
                            Position = tokenization.EndOfInputPosition,
                        };
                        foreach (var branch in info.Branches)
                        {
                            rejects.Add(branch.NodeMatch, err);
                        }
                    }
                    else // we have valid tokens
                    {
                        var offendingToken = tokenization.InputElements.First();

                        var err = new ParserError {
                            ErrorType = ParserError.ExcessRemainingInput,
                            LastValidMatchingNode = info.Source.Node,
                            Position = offendingToken.Position,
//                            OffendingToken = offendingToken,
                        };

                        RejectEndCandidate(info, rejects, ends, err);

                        foreach (var branch in info.Branches)
                        {
                            branches2.Enqueue(new Tuple<NodeMatch<TElement>, MatchStack<TElement>, ParseInfo>(
                                branch.NodeMatch,
                                branch.MatchStack,
                                info),
                                info.Source.Token.IndexOfNextTokenization);
                        }
                    }
                }

                while (branches2.Count > 0)
                {
                    var branchtuple = branches2.Dequeue();
                    var branchnm = branchtuple.Item1;
                    var branchstack = branchtuple.Item2;
                    var info = branchtuple.Item3;

                    var tokenization = tokenSource.GetInputAtLocation(info.Source.Token.IndexOfNextTokenization);

                    if (!tokenization.Errors.ContainsNonWarnings() &&
                        !tokenization.EndOfInput)
                    {
                        // we have valid tokens
                        var offendingToken = tokenization.InputElements.First();
                        var err = new ParserError {
                            ErrorType = ParserError.ExcessRemainingInput,
                            LastValidMatchingNode = info.Source.Node,
                            Position = offendingToken.Position,
//                            OffendingToken = offendingToken,
                        };

                        RejectEndCandidate(info, rejects, ends, err);

                        // try to match branch to tokens
                        bool matched = false;
                        foreach (var intoken in tokenization.InputElements)
                        {
                            if (BranchTipMatchesInputElement(branchnm, intoken))
                            {
                                var newNext = branchnm.CloneWithNewInputElement(intoken);
                                nextSources.Add(pair(newNext, branchstack));
                                matched = true;
                            }
                        }

                        ParserError err2 = null;
                        // if the branch didn't match, reject it with InvalidToken
                        // otherwise, reject it with null since it's a duplicate
                        if (!matched)
                        {
                            err2 = new ParserError {
                                ErrorType = ParserError.InvalidToken,
                                LastValidMatchingNode = info.Source.Node,
//                                OffendingToken = offendingToken,
                                ExpectedNodes = info.Source.Node.NextNodes,
                                Position = offendingToken.Position,
                            };
                        }

                        rejects.Add(branchnm, err2);
                    }
                }

                foreach (var next in nextSources)
                {
                    sources.Enqueue(next, next.NodeMatch.Token.IndexOfNextTokenization);
//                    Logger.WriteLine("Enqueuing source with next index {0}", next.NodeMatch.Token.IndexOfNextTokenization);
                }
            }

            if (ends.Count > 0)
            {
                foreach (var reject in rejects)
                {
                    StripReject(reject.NodeMatch);
                }
            }
            else
            {
                if (rejects.Count > 0)
                {
                    IEnumerable<Error> errorsToUse = null;
                    foreach (var reject in (rejects as IEnumerable<NodeMatchErrorPair<TElement>>).Reverse())
                    {
                        if (reject.Errors != null && reject.Errors.Any())
                        {
                            errorsToUse = reject.Errors;
                            break;
                        }
                    }

                    if (errorsToUse != null)
                    {
                        errors.AddRange(errorsToUse);
                    }
                    else
                    {
                        throw new InvalidOperationException("No errors among the rejects");
                    }
                }
                else
                {
                    // failed to start?
                    throw new NotImplementedException();
                }
            }

            return ends.ToArray();
        }

        void RejectEndCandidate(ParseInfo info, List<NodeMatchErrorPair<TElement>> rejects, List<NodeMatch<TElement>> ends, Error err)
        {
            if (info.EndCandidate != null)
            {
                ends.Remove(info.EndCandidate);
                rejects.Add(info.EndCandidate, err);
                info.EndCandidate = null;
            }
        }
        void RejectEndCandidate(ParseInfo info, List<NodeMatchErrorPair<TElement>> rejects, List<NodeMatch<TElement>> ends, IEnumerable<Error> errors)
        {
            if (info.EndCandidate != null)
            {
                ends.Remove(info.EndCandidate);
                rejects.Add(info.EndCandidate, errors);
                info.EndCandidate = null;
            }
        }

        ParseInfo GetParseInfoFromSource(NodeMatchStackPair<TElement> source)
        {
            var info = new ParseInfo();
            info.SourcePair = source;

            var currents = new Queue<NodeMatchStackPair<TElement>>();

            currents.Enqueue(info.SourcePair);

            // find all ends
            var enders = new List<NodeMatchStackPair<TElement>>();
            if (info.Source.Transition != NodeMatch<TElement>.TransitionType.Root)
            {
                var ender = info.SourcePair;

                while (ender.NodeMatch != null &&
                       ender.MatchStack != null &&
                       ender.NodeMatch.Node.IsEndNode)
                {
                    ender = ender.CreateEndDefMatch();
                    enders.Add(ender);
                }

                if (ender.NodeMatch != null &&
                    ender.MatchStack == null)
                {
                    info.EndCandidate = ender.NodeMatch;
                }
            }

            foreach (var ender in enders)
            {
                currents.Enqueue(ender);
            }

            //find all branches
            info.Branches = new List<NodeMatchStackPair<TElement>>();
            while (currents.Count > 0)
            {
                var current = currents.Dequeue();
                var cur = current.NodeMatch;
                var curstack = current.MatchStack;

                if (IsBranchTip(cur) &&
                    cur != info.Source)
                {
                    info.Branches.Add(current);
                    continue;
                }

                if (cur.DefRef.IsTokenized ||
                    cur.Transition == NodeMatch<TElement>.TransitionType.EndDef)
                {
                    foreach (var next in cur.Node.NextNodes)
                    {
                        var nm = new NodeMatch<TElement>(next, NodeMatch<TElement>.TransitionType.Follow, cur);
                        currents.Enqueue(pair(nm, curstack));
                    }
                }
                else
                {
                    var nextStack = new MatchStack<TElement>(cur, curstack);
                    foreach (var start in (cur.Node as DefRefNode).DefRef.StartNodes)
                    {
                        var nm = new NodeMatch<TElement>(start, NodeMatch<TElement>.TransitionType.StartDef, cur);
                        currents.Enqueue(pair(nm, nextStack));
                    }
                }
            }

            return info;
        }

        protected virtual bool IsBranchTip(NodeMatch<TElement> cur)
        {
            return cur.DefRef.IsTokenized;
        }
        protected abstract bool BranchTipMatchesInputElement(NodeMatch<TElement> branchTip, TElement inputElement);


        static Span[] MakeSpans(IEnumerable<NodeMatch<TElement>> matchTreeLeaves)
        {
            var lists = new List<List<NodeMatch<TElement>>>();
            foreach (NodeMatch<TElement> leaf in matchTreeLeaves)
            {
                NodeMatch<TElement> cur = leaf;
                var list = new List<NodeMatch<TElement>>();

                while (cur != null)
                {
                    list.Add(cur);
                    cur = cur.Previous;
                }
                list.Reverse();

                lists.Add(list);
            }

            var spans = new List<Span>();
            foreach (List<NodeMatch<TElement>> list in lists)
            {
                var stack = new Stack<Span>();

                Span rootSpan = null;

                foreach (NodeMatch<TElement> nm in list)
                {
                    if (nm.Transition == NodeMatch<TElement>.TransitionType.EndDef)
                    {
                        rootSpan = stack.Pop();
                    }
                    else if (!nm.DefRef.IsTokenized)
                    {
                        var s = new Span();
                        s.Node = nm.Node;
                        if (stack.Count > 0)
                        {
                            stack.Peek().Subspans.Add(s);
                        }
                        stack.Push(s);
                    }
                    else
                    {
                        var s = new Span();
                        s.Node = nm.Node;
                        s.Value = nm.Token.Value;
                        stack.Peek().Subspans.Add(s);
                    }
                }

                spans.Add(rootSpan);
            }

            return spans.ToArray();
        }

        public static NodeMatchStackPair<TElement> pair(NodeMatch<TElement> nodeMatch, MatchStack<TElement> matchStack)
        {
            return new NodeMatchStackPair<TElement>{NodeMatch = nodeMatch, MatchStack = matchStack};
        }


        public static void StripReject(NodeMatch<TElement> reject)
        {
            NodeMatch<TElement> cur = reject;
            NodeMatch<TElement> next = cur;
            while (cur != null &&
                   cur.Nexts.Count < 2)
            {
                next = cur;
                cur = cur.Previous;
            }
            if (cur != null && cur != next)
            {
                next.Previous = null;
            }
        }
    }
}

