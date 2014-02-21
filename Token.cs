using System;

namespace MetaphysicsIndustries.Giza
{
    public struct Token
    {
        public Token(Definition definition=null,
                     InputPosition startPosition=new InputPosition(),
                     string value="",
                     int indexOfNextTokenization=-1)
        {
            if (indexOfNextTokenization < 0)
            {
                indexOfNextTokenization = startPosition.Index + (value ?? "").Length;
            }

            Definition = definition;
            StartPosition = startPosition;
            Value = value;
            IndexOfNextTokenization = indexOfNextTokenization;
        }

        public Definition Definition;
        public InputPosition StartPosition;
        public string Value;
        public int IndexOfNextTokenization;
    }
}

