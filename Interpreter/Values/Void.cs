using System.Collections.Generic;
using Bloc.Results;

namespace Bloc.Values
{
    public sealed class Void : Value
    {
        private Void() { }

        public static Void Value { get; } = new();

        internal override ValueType GetType() => ValueType.Void;

        internal override void Assign()
        {
            throw new Throw("You cannot assign void to a variable");
        }

        public override bool Equals(Value other)
        {
            return other is Void;
        }

        internal static Void Construct(List<Value> values)
        {
            if (values.Count != 0)
                throw new Throw($"'void' does not have a constructor that takes {values.Count} arguments");

            return new();
        }

        public override string ToString(int _)
        {
            return "void";
        }
    }
}