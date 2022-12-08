using System.Collections.Generic;
using Bloc.Memory;
using Bloc.Pointers;
using Bloc.Results;

namespace Bloc.Values
{
    public sealed class Reference : Value
    {
        internal Reference() => Pointer = new VariablePointer(null);

        internal Reference(Pointer pointer) => Pointer = pointer;

        internal Pointer Pointer { get; }

        internal override ValueType GetType() => ValueType.Reference;

        internal static Reference Construct(List<Value> values, Call call)
        {
            return values.Count switch
            {
                0 => new(),
                1 => values[0] switch
                {
                    Null => new(),
                    String @string => new(call.Get(@string.Value).Resolve()),
                    Reference reference => reference,
                    _ => throw new Throw($"'reference' does not have a constructor that takes a '{values[0].GetType().ToString().ToLower()}'")
                },
                _ => throw new Throw($"'reference' does not have a constructor that takes {values.Count} arguments")
            };
        }

        internal override string ToString(int _)
        {
            return "[reference]";
        }

        public override int GetHashCode()
        {
            return 0;
        }

        public override bool Equals(object other)
        {
            if (other is not Reference reference)
                return false;

            return Pointer.Equals(reference.Pointer);
        }
    }
}
