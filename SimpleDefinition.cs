﻿using System;
using System.Collections.Generic;
using System.Text;
using MetaphysicsIndustries.Collections;
using System.Diagnostics;

namespace MetaphysicsIndustries.Giza
{
    public class SimpleDefinition
    {
        private static int __id = 0;
        public readonly int _id;

        public SimpleDefinition()
        {
            _id = __id;
            __id++;
        }

        public string Name;
        public Set<SimpleNode> Nodes = new Set<SimpleNode>();
        public SimpleNode start;
        public SimpleNode end;

        public bool IgnoreWhitespace = false;
        public bool IgnoreCase = false;

        public override string ToString()
        {
            return string.Format("[{0}] {1}, {2} nodes", _id, Name, Nodes.Count);
        }
    }
}
