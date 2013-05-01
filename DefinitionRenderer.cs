using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace MetaphysicsIndustries.Giza
{
    public class DefinitionRenderer
    {
        public string RenderDefinitionsAsCSharpClass(string className, IEnumerable<Definition> _defs)
        {
            List<Definition> defs = new List<Definition>(_defs);
            
            Dictionary<Definition, string> defnames = new Dictionary<Definition, string>();
            
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine();
            sb.AppendLine("namespace MetaphysicsIndustries.Giza");
            sb.AppendLine("{");
            sb.AppendFormat("    public class {0}", RenderIdentifier(className));
            sb.AppendLine();
            sb.AppendLine("    {");
            sb.AppendLine("        List<Definition> Definitions = new List<Definition>();");
            sb.AppendLine();
            
            foreach (Definition def in defs)
            {
                string name = string.Format("def_{0}_{1}", def._id, RenderIdentifier(def.Name));
                
                defnames[def] = name;
                
                sb.AppendFormat(
                    "        Definition {0} = new Definition({1});",
                    name,
                    RenderString(def.Name));
                sb.AppendLine();
            }
            
            sb.AppendLine();
            
            Dictionary<Node, string> nodenames = new Dictionary<Node, string>();
            
            foreach (Definition def in defs)
            {
                foreach (Node node in def.Nodes)
                {
                    string name = string.Format("node_{0}_{1}_{2}", node._id, RenderIdentifier(def.Name), RenderNodeNameAsIdentifier(node.Tag));
                    
                    nodenames[node] = name;
                    
                    string type = "";
                    if (node is StartNode)
                    {
                        type = "StartNode";
                    }
                    else if (node is EndNode)
                    {
                        type = "EndNode";
                    }
                    else if (node is CharNode)
                    {
                        type = "CharNode";
                    }
                    else if (node is DefRefNode)
                    {
                        type = "DefRefNode";
                    }
                    
                    sb.AppendFormat("        {0} {1};", type, name);
                    sb.AppendLine();
                }
            }
            
            sb.AppendLine();
            sb.AppendFormat("        public {0}()", RenderIdentifier(className));
            sb.AppendLine();
            sb.AppendLine("        {");
            
            foreach (Definition def in defs)
            {
                sb.AppendFormat("Definitions.Add({0});", defnames[def]);
                sb.AppendLine();
            }
            
            sb.AppendLine();
            
            foreach (Definition def in defs)
            {
                sb.AppendFormat("{0}.IgnoreCase = {1};", defnames[def], (def.IgnoreCase ? "true" : "false"));
                sb.AppendLine();
                sb.AppendFormat("{0}.IgnoreWhitespace = {1};", defnames[def], (def.IgnoreWhitespace ? "true" : "false"));
                sb.AppendLine();
                sb.AppendFormat("{0}.Contiguous = {1};", defnames[def], (def.Contiguous ? "true" : "false"));
                sb.AppendLine();
                
                foreach (Node node in def.Nodes)
                {
                    string name = nodenames[node];
                    
                    if (node is StartNode)
                    {
                        sb.AppendFormat("{0} = new StartNode();", name);
                    }
                    else if (node is EndNode)
                    {
                        sb.AppendFormat("{0} = new EndNode();", name);
                    }
                    else if (node is CharNode)
                    {
                        sb.AppendFormat(
                            "{0} = new CharNode(CharClass.FromUndelimitedCharClassText({1}), {2});",
                            name,
                            RenderString((node as CharNode).CharClass.ToUndelimitedString()),
                            RenderString(node.Tag));
                    }
                    else if (node is DefRefNode)
                    {
                        sb.AppendFormat(
                            "{0} = new DefRefNode({1}, {2});",
                            name,
                            defnames[(node as DefRefNode).DefRef],
                            RenderString(node.Tag));
                    }
                    sb.AppendLine();
                }
                foreach (Node node in def.Nodes)
                {
                    sb.AppendFormat("{0}.Nodes.Add({1});", defnames[def], nodenames[node]);
                    sb.AppendLine();
                }
                
                sb.AppendFormat("{0}.start = ", defnames[def]);
                if (def.start == null)
                {
                    sb.Append("null");
                }
                else
                {
                    sb.Append(nodenames[def.start]);
                }
                sb.AppendLine(";");
                
                sb.AppendFormat("{0}.end = ", defnames[def]);
                if (def.end == null)
                {
                    sb.Append("null");
                }
                else
                {
                    sb.Append(nodenames[def.end]);
                }
                sb.AppendLine(";");
                
                foreach (Node node in def.Nodes)
                {
                    foreach (Node next in node.NextNodes)
                    {
                        sb.AppendFormat("{0}.NextNodes.Add({1});", nodenames[node], nodenames[next]);
                        sb.AppendLine();
                    }
                }
                sb.AppendLine();
            }
            
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine("}");
            sb.AppendLine();
            
            return sb.ToString();
        }
        
        static string RenderIdentifier(string s)
        {
            return Regex.Replace(s, @"[^\w\d_]", "_");
        }
        
        static string RenderString(string s)
        {
            string s2 = s;
            
            s2 = s2.Replace("\"", "\\\"");
            s2 = s2.Replace("\\", "\\\\");
            s2 = s2.Replace("\r", "\\r");
            s2 = s2.Replace("\n", "\\n");
            s2 = s2.Replace("\t", "\\t");
            
            return "\"" + s2 + "\"";
        }
        
        static string RenderNodeNameAsIdentifier(string name)
        {
            StringBuilder sb = new StringBuilder();
            
            foreach (char ch in name)
            {
                if (char.IsLetterOrDigit(ch) || ch == '_')
                {
                    sb.Append(ch.ToString());
                }
                else
                {
                    sb.Append("_");
                    sb.Append(Convert.ToInt32(ch).ToString("X4"));
                    sb.Append("_");
                }
            }
            
            return sb.ToString();
        }
    }
}

