using System.Collections.Generic;
using Bloc.Results;

namespace Bloc.Values
{
    public sealed class Null : Value
    {
        private Null() { }

        public static Null Value { get; } = new();

        internal override ValueType GetType() => ValueType.Null;

        internal static Null Construct(List<Value> values)
        {
            if (values.Count != 0)
                throw new Throw($"'null' does not have a constructor that takes {values.Count} arguments");

            return Value;
        }

        internal override string ToString(int _)
        {
            return "null";
        }

        public override int GetHashCode()
        {
            return 0;
        }

        public override bool Equals(object other)
        {
            return other is Null;
        }
    }
}