using System.Collections.Generic;
using Bloc.Results;

namespace Bloc.Values
{
    public sealed class Void : Value
    {
        private Void() { }

        public static Void Value { get; } = new();

        internal override ValueType GetType() => ValueType.Void;

        internal static Void Construct(List<Value> values)
        {
            if (values.Count != 0)
                throw new Throw($"'void' does not have a constructor that takes {values.Count} arguments");

            return Value;
        }

        internal override string ToString(int _)
        {
            return "void";
        }

        public override int GetHashCode()
        {
            return 0;
        }

        public override bool Equals(object other)
        {
            return other is Void;
        }
    }
}