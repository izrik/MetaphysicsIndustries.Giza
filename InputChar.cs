using System;
using System.Collections.Generic;

namespace MetaphysicsIndustries.Giza
{
    public struct InputChar : IInputElement
    {
        public InputChar(char value, InputPosition position)
        {
            Value = value;
            Position = position;
            IndexOfNextElement = position.Index + 1;
        }

        public readonly char Value;
        public readonly InputPosition Position;
        public readonly int IndexOfNextElement;

        #region IInputElement implementation
        string IInputElement.Value { get { return Value.ToString(); } }
        InputPosition IInputElement.Position { get { return Position; } }
        int IInputElement.IndexOfNextElement { get { return IndexOfNextElement; } }
        #endregion
    }
}

