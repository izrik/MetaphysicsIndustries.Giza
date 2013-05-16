﻿
/*****************************************************************************
 *                                                                           *
 *  SupergrammarSpanner.cs                                                   *
 *  6 July 2010                                                              *
 *  Project: MetaphysicsIndustries.Giza                                      *
 *  Written by: Richard Sartor                                               *
 *  Copyright © 2010 Metaphysics Industries, Inc.                            *
 *                                                                           *
 *****************************************************************************/

using System;
using System.Collections.Generic;
using MetaphysicsIndustries.Collections;

namespace MetaphysicsIndustries.Giza
{
    public class SupergrammarSpanner
    {
        public Grammar GetGrammar(string input)
        {
            Supergrammar supergrammar = new Supergrammar();
            Definition.__id = 0;
            Spanner spanner = new Spanner();
            Span[] s2 = spanner.Process2(supergrammar.Definitions, "grammar", input);

            DefinitionBuilder db = new DefinitionBuilder();
            Definition[] defs = db.BuildDefinitions(supergrammar, s2[0]);

            Grammar grammar = new Grammar();
            grammar.Definitions.AddRange(defs);

            return grammar;
        }
    }
}


