using System;
using System.Collections.Generic;

namespace MetaphysicsIndustries.Giza
{
    public class Parser
    {
        public Parser(Definition definition)
        {
            if (definition == null) throw new ArgumentNullException("definition");

            _definition = definition;
            _tokenizer = new Tokenizer(_definition.ParentGrammar);
        }

        Definition _definition;
        Tokenizer _tokenizer;

        public Span[] Parse(string input)
        {
            var sources = new Queue<NodeMatchStackPair>();
            var ends = new List<NodeMatch>();
            var rootDef = new Definition("$rootDef");
            var rootNode = new DefRefNode(_definition, "$rootNode");
            rootDef.Nodes.Add(rootNode);
            rootDef.StartNodes.Add(rootNode);
            rootDef.EndNodes.Add(rootNode);
            var root = new NodeMatch(rootNode, NodeMatch.TransitionType.Root, null);

            sources.Enqueue(pair(root, null));

            while (sources.Count > 0)
            {
                var nextSources = new List<NodeMatchStackPair>();

                while (sources.Count > 0)
                {
                    var sourcepair = sources.Dequeue();
                    var source = sourcepair.NodeMatch;

                    bool shouldRejectSource = true;

                    //get all tokens, starting at end of source's token
                    var errors = new List<Error>();
                    var intokens = _tokenizer.GetTokensAtLocation(
                                        input, 
                                        source.Token.StartIndex + source.Token.Length, 
                                        errors);
                    //if we get a token, set shouldRejectSource to false
                    if (intokens != null && 
                        intokens.Length > 0)
                    {
                        shouldRejectSource = false;
                    }

                    //find all branches
                    var branches = new List<NodeMatchStackPair>();
                    var currents = new Queue<NodeMatchStackPair>();

                    currents.Enqueue(sourcepair);

                    while (currents.Count > 0)
                    {
                        NodeMatchStackPair current = currents.Dequeue();
                        NodeMatch cur = current.NodeMatch;
                        MatchStack curstack = current.MatchStack;

                        if (cur.Node.IsEndNode &&
                            cur.Transition != NodeMatch.TransitionType.Root &&
                            (cur == source ||
                             cur.Transition == NodeMatch.TransitionType.EndDef))
                        {
                            if (curstack == null)
                            {
                                if (intokens != null && intokens.Length > 0)
                                {
//                                    Reject(cur);
                                }
                                else
                                {
                                    ends.Add(cur);
                                    shouldRejectSource = false;
                                }
                            }
                            else
                            {
                                NodeMatch nm = new NodeMatch(curstack.Node, NodeMatch.TransitionType.EndDef, cur);
                                nm.StartDef = curstack.NodeMatch;
                                currents.Enqueue(pair(nm, curstack.Parent));
                            }
                        }

                        if (cur.DefRef.IsTokenized && 
                            cur != source)
                        {
                            branches.Add(current);
                            shouldRejectSource = false;
                            continue;
                        }


                        if (cur.DefRef.IsTokenized ||
                            cur.Transition == NodeMatch.TransitionType.EndDef)
                        {
                            foreach (var next in cur.Node.NextNodes)
                            {
                                NodeMatch nm = new NodeMatch(next, NodeMatch.TransitionType.Follow, cur);
                                currents.Enqueue(pair(nm, curstack));
                            }
                        }
                        else
                        {
                            var nextStack = new MatchStack(cur, curstack);
                            foreach (var start in (cur.Node as DefRefNode).DefRef.StartNodes)
                            {
                                NodeMatch nm = new NodeMatch(start, NodeMatch.TransitionType.StartDef, cur);
                                currents.Enqueue(pair(nm, nextStack));
                            }
                        }
                    }

                    //if no tokens, reject source and continue
                    //if no branches, reject source and continue
                    //however, if ends, then dont reject source
                    if (shouldRejectSource)
                    {
                        foreach (var branch in branches)
                        {
                            Reject(branch.NodeMatch);
                        }

                        Reject(source);
                        continue;
                    }

                    foreach (var branch in branches)
                    {
                        var branchnm = branch.NodeMatch;
                        var branchstack = branch.MatchStack;

                        foreach (var intoken in intokens)
                        {
                            if ((branchnm.Node is DefRefNode) && 
                                (branchnm.Node as DefRefNode).DefRef == intoken.Definition)
                            {
                                var newNext = branchnm.CloneWithNewToken(intoken);
                                nextSources.Add(pair(newNext, branchstack));
                            }
                        }

                        Reject(branch.NodeMatch);
                    }
                }

                foreach (var next in nextSources)
                {
                    sources.Enqueue(next);
                }
            }

            return MakeSpans(ends, input);
        }

        static Span[] MakeSpans(IEnumerable<NodeMatch> matchTreeLeaves, string input)
        {
            List<List<NodeMatch>> lists = new List<List<NodeMatch>>();
            foreach (NodeMatch leaf in matchTreeLeaves)
            {
                NodeMatch cur = leaf;
                List<NodeMatch> list = new List<NodeMatch>();

                while (cur != null)
                {
                    list.Add(cur);
                    cur = cur.Previous;
                }
                list.Reverse();

                lists.Add(list);
            }

            List<Span> spans = new List<Span>();
            foreach (List<NodeMatch> list in lists)
            {
                Stack<Span> stack = new Stack<Span>();

                Span rootSpan = null;

                foreach (NodeMatch nm in list)
                {
                    if (nm.Transition == NodeMatch.TransitionType.EndDef)
                    {
                        rootSpan = stack.Pop();
                    }
                    else if (!nm.DefRef.IsTokenized)
                    {
                        Span s = new Span();
                        s.Node = nm.Node;
                        if (stack.Count > 0)
                        {
                            stack.Peek().Subspans.Add(s);
                        }
                        stack.Push(s);
                    }
                    else
                    {
                        Span s = new Span();
                        s.Node = nm.Node;
                        s.Value = input.Substring(nm.Token.StartIndex, nm.Token.Length);
                        stack.Peek().Subspans.Add(s);
                    }
                }

                spans.Add(rootSpan);
            }

            return spans.ToArray();
        }

        public static NodeMatchStackPair pair(NodeMatch nodeMatch, MatchStack matchStack)
        {
            return new NodeMatchStackPair{NodeMatch = nodeMatch, MatchStack = matchStack};
        }

        List<NodeMatch> _rejects = new List<NodeMatch>();
        public void Reject(NodeMatch reject)
        {
            _rejects.Add(reject);
            NodeMatch next = reject;
            NodeMatch cur = reject;
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

//            while (reject != null &&
//                    reject.Nexts.Count < 1)
//            {
//                var temp = reject;
//                reject = reject.Previous;
//                temp.Previous = null;
//            }
        }
    }
}

