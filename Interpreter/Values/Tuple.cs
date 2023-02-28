using System.Collections.Generic;
using System.Linq;
using Bloc.Results;
using Bloc.Variables;

namespace Bloc.Values
{
    public sealed class Tuple : Value
    {
        public Tuple() => Variables = new();

        public Tuple(List<IVariable> value)
        {
            Variables = value;
        }

        public Tuple(List<Value> value)
        {
            Variables = value
                .Select(x => new TupleVariable(x))
                .ToList<IVariable>();
        }

        public List<IVariable> Variables { get; }

        internal override ValueType GetType() => ValueType.Tuple;

        internal static Tuple Construct(List<Value> values)
        {
            return values.Count switch
            {
                0 => new(),
                1 => values[0] switch
                {
                    Null => new(),
                    Range range => new(),
                    Array array => new(array.Variables
                        .Select(x => x.Value.Copy())
                        .ToList()),
                    Struct @struct => new(@struct.Variables
                        .OrderBy(x => x.Key)
                        .Select(x => x.Value.Value.Copy())
                        .ToList()),
                    Tuple tuple => tuple,
                    _ => throw new Throw($"'iter' does not have a constructor that takes a '{values[0].GetType().ToString().ToLower()}'")
                },
                _ => throw new Throw($"'iter' does not have a constructor that takes {values.Count} arguments")
            };
        }

        internal override Value Copy()
        {
            return new Tuple(Variables
                .Select(x => new TupleVariable(x.Value.Copy()))
                .ToList<IVariable>());
        }

        public override string ToString()
        {
            return "(" + string.Join(", ", Variables.Select(v => v.Value)) + ")";
        }

        public override int GetHashCode()
        {
            return System.HashCode.Combine(Variables.Count);
        }

        public override bool Equals(object other)
        {
            if (other is not Tuple tuple)
                return false;

            if (Variables.Count != tuple.Variables.Count)
                return false;

            for (var i = 0; i < Variables.Count; i++)
                if (Variables[i].Value != tuple.Variables[i].Value)
                    return false;

            return true;
        }
    }
}