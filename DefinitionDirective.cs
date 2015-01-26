using System;
using System.Collections.Generic;
using System.Text;

using System.Text.RegularExpressions;
using System.Globalization;

namespace MetaphysicsIndustries.Giza
{
    public enum DefinitionDirective
    {
        MindWhitespace,
        IgnoreCase,
        Atomic,
        Token,
        Subtoken,
        Comment,
        Whitespace,
    }

    public static class DefinitionDirectiveHelper
    {
        public static string GetName(this DefinitionDirective directive)
        {
            switch (directive)
            {
            case DefinitionDirective.MindWhitespace:
                return "mind whitespace";
            case DefinitionDirective.IgnoreCase:
                return "ignore case";
            case DefinitionDirective.Atomic:
                return "atomic";
            case DefinitionDirective.Token:
                return "token";
            case DefinitionDirective.Subtoken:
                return "subtoken";
            case DefinitionDirective.Comment:
                return "comment";
            case DefinitionDirective.Whitespace:
                return "whitespace";
            }

            throw new ArgumentOutOfRangeException("directive",
                string.Format("Unknown DefinitionDirective value: {0}",
                    (int)directive));
        }
    }
}
