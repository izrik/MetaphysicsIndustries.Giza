﻿
// MetaphysicsIndustries.Giza - A Parsing System
// Copyright (C) 2008-2020 Metaphysics Industries, Inc.
//
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 3 of the License, or (at your option) any later version.
//
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301
// USA

using System;
using System.Collections.Generic;
using MetaphysicsIndustries.Giza;
using NCommander;
using System.IO;
using System.Linq;

namespace giza
{
    public class RenderReplCommand : ReplCommand
    {
        public RenderReplCommand(Dictionary<string, DefinitionExpression> env)
            : base(env)
        {
            Name = "render";
            Description = "Convert definitions to state machine format and render the state machines to a C# class.";
            Params = new [] {
                new Parameter {
                    Name="class-name",
                    ParameterType=ParameterType.String,
                    Description="The name of the C# class to generate",
                },
                new Parameter {
                    Name="def-names",
                    ParameterType=ParameterType.StringArray,
                    Description="The definitions to render",
                },
            };
            Options = new [] {
                new Option {
                    Name="tokenized",
                    Description="The grammar is tokenized",
                },
                new Option {
                    Name="singleton",
                    Description="Add a single static readonly field to the class, as a default instance",
                },
                new Option {
                    Name="namespace",
                    Type=ParameterType.String,
                    Description="The namespace in which the C# class is defined (default is 'MetaphysicsIndustries.Giza')",
                },
                new Option {
                    Name="to-file",
                    Type=ParameterType.String,
                    Description="Save the c# class to the specified file, instead of printing it out",
                },
            };
        }

        protected override void InternalExecute(Dictionary<string, object> args)
        {
            var tokenized = (bool)args["tokenized"];
            var ns = (string)args["namespace"] ?? "MetaphysicsIndustries.Giza";
            var singleton = (bool)args["singleton"];
            var toFile = (string)args["to-file"];

            var className = (string)args["class-name"];
            var defnames = (string[])args["def-names"];

            if (defnames == null || defnames.Length < 1)
            {
                Console.WriteLine("Error: No definitions specified");
                return;
            }

            var someAreMissing = false;
            foreach (var name in defnames)
            {
                if (!Env.ContainsKey(name))
                {
                    Console.WriteLine("Error: There is no definition named \"{0}\".", name);
                    someAreMissing = true;
                }
            }

            if (someAreMissing) return;

            var defs = defnames.Select(name => Env[name]).ToArray();
            var alldefs = SaveReplCommand.GetAllReferencedDefinitions(defs, Env, ref someAreMissing);

            if (someAreMissing) return;

            Grammar g;
            if (tokenized)
            {
                var tgb = new TokenizedGrammarBuilder();
                g = tgb.BuildTokenizedGrammar(alldefs);
            }
            else
            {
                var db = new DefinitionBuilder();
                g = new Grammar(db.BuildDefinitions(alldefs));
            }

            var dr = new DefinitionRenderer();
            var cs = dr.RenderDefinitionsAsCSharpClass(className, g.Definitions, ns, singleton);

            if (!string.IsNullOrEmpty(toFile))
            {
                File.WriteAllText(toFile, cs);
            }
            else
            {
                Console.WriteLine(cs);
            }
        }
    }
}

