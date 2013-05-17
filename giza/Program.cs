using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MetaphysicsIndustries.Giza;
using System.IO;

namespace giza
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                ShowUsage();
            }
            else if (args[0].ToLower() == "--version" ||
                     args[0].ToLower() == "-v")
            {

            }
            else if (args[0].ToLower() == "--help")
            {
                ShowUsage();
            }
            else if (args[0].ToLower() == "--super")
            {
                if (args.Length > 1)
                {
                    SupergrammarSpanner s = new SupergrammarSpanner();
                    string gfile;
                    if (args[1] == "-")
                    {
                        gfile = new StreamReader(Console.OpenStandardInput()).ReadToEnd();
                    }
                    else
                    {
                        gfile = File.ReadAllText(args[1]);
                    }
                    if (args.Length > 2 && args[2] == "-2")
                    {
                        Supergrammar supergrammar = new Supergrammar();
                        Definition.__id = 0;
                        Spanner spanner = new Spanner();
                        Span[] ss = spanner.Process(supergrammar.Definitions.ToArray(), "grammar", gfile);
                        DefinitionBuilder db = new DefinitionBuilder();
                        Definition.__id = 0;
                        DefinitionRenderer dr = new DefinitionRenderer();
                        foreach (Span span in ss)
                        {
                            Definition.__id = 0;
                            Definition[] dd2 = db.BuildDefinitions(supergrammar, span);
                            string class2 = dr.RenderDefinitionsAsCSharpClass("FromBuildDefs2", dd2);
                            class2 = class2;
                        }
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }
                else
                {
                    ShowUsage();
                }
            }
            else if (args[0].ToLower() == "--render")
            {
                if (args.Length > 2)
                {
                    SupergrammarSpanner s = new SupergrammarSpanner();
                    string gfile;
                    if (args[1] == "-")
                    {
                        gfile = new StreamReader(Console.OpenStandardInput()).ReadToEnd();
                    }
                    else
                    {
                        gfile = File.ReadAllText(args[1]);
                    }
                    Grammar g = s.GetGrammar(gfile);

                    string className = args[2];

                    DefinitionRenderer dr = new DefinitionRenderer();
                    Console.Write(dr.RenderDefinitionsAsCSharpClass(className, g.Definitions));
                }
                else
                {
                    ShowUsage();
                }
            }
            else if (args.Length < 3)
            {
                ShowUsage();
            }
            else
            {
                SupergrammarSpanner spanner = new SupergrammarSpanner();
                string grammarFile = File.ReadAllText(args[0]);
                Grammar g = spanner.GetGrammar(grammarFile);

                string input;
                if (args[2] == "-")
                {
                    input = new StreamReader(Console.OpenStandardInput()).ReadToEnd();
                }
                else
                {
                    input = File.ReadAllText(args[2]);
                }

                Spanner gs = new Spanner();
                if (args.Length > 3 && args[3] == "-2")
                {
                    Span[] ss = gs.Process(g.Definitions.ToArray(), args[1], input);
                    DefinitionBuilder db = new DefinitionBuilder();
                    foreach (Span span in ss)
                    {
//                        Definition[] dd = db.BuildDefinitions2(g, span);
                    }
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
        }

        private static int CountLines(string input, int length)
        {
            int n = 1;
            int i;
            for (i=0;i<length;i++)
            {
                if (input[i] == '\n') n++;
            }
            return n;
        }
        private static int CountCharactersOnLine(string input, int endIndex)
        {
            int n = 0;
            int i = endIndex;
            do
            {
                if (input[i] != '\r') n++;
                i--;
            } while (i >= 0 && input[i] != '\n');

            return n;
        }

        static void ShowUsage()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("    giza [options]");
            Console.WriteLine("    giza [options] [GRAMMAR FILE] [START SYMBOL] [FILE]");
            Console.WriteLine("    giza --super [GRAMMAR FILE]");
            //Console.WriteLine("    giza --print-super");
            //Console.WriteLine("    giza --compile [GRAMMAR FILE] [START SYMBOL] [OUTPUT EXE]");
            Console.WriteLine("    giza --render [GRAMMAR FILE] [CLASS NAME]");
            Console.WriteLine();
            Console.WriteLine("Reads grammar files and parses input.");
            Console.WriteLine();
            Console.WriteLine("    --version, -[vV]  Print version and exit successfully.");
            Console.WriteLine("    --help,           Print this help and exit successfully.");
            Console.WriteLine("    --super,          Process the grammar file only.");
            //Console.WriteLine("    --print-super,    Print the supergrammar and exit.");
            //Console.WriteLine("    --compile,      Print the supergrammar and exit.");
            Console.WriteLine("    --render,         Process the grammar file and print its definitions as a C# class.");
            Console.WriteLine();
            Console.WriteLine("If \"-\" is given for FILE, or for GRAMMAR FILE given to --super, then it is read from standard input.");
            Console.WriteLine();
        }

    }
}
