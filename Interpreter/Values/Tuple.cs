using System.Collections.Generic;
using System.Linq;
using Bloc.Pointers;
using Bloc.Results;
using Bloc.Variables;

namespace Bloc.Values
{
    public class Tuple : Value
    {
        public Tuple() => Values = new();

        public Tuple(List<IPointer> value) => Values = value;

        public List<IPointer> Values { get; }

        public override ValueType GetType() => ValueType.Tuple;

        internal override Value Copy()
        {
            return new Tuple(Values.Select(v => v.Value.Copy()).ToList<IPointer>());
        }

        internal override void Assign()
        {
            foreach (var value in Values)
                value.Value.Assign();
        }

        public override bool Equals(Value other)
        {
            if (other is not Tuple tuple)
                return false;

            if (Values.Count != tuple.Values.Count)
                return false;

            for (var i = 0; i < Values.Count; i++)
                if (!Values[i].Value.Equals(tuple.Values[i].Value))
                    return false;

            return true;
        }

        public override T Implicit<T>()
        {
            if (typeof(T) == typeof(Null))
                return (Null.Value as T)!;

            if (typeof(T) == typeof(Bool))
                return (Bool.True as T)!;

            if (typeof(T) == typeof(String))
                return (new String(ToString()) as T)!;

            if (typeof(T) == typeof(Tuple))
                return (this as T)!;

            throw new Throw($"Cannot implicitly cast tuple as {typeof(T).Name.ToLower()}");
        }

        public override Value Explicit(ValueType type)
        {
            return type switch
            {
                ValueType.Null => Null.Value,
                ValueType.Bool => Bool.True,
                ValueType.String => new String(ToString()),
                ValueType.Array => new Array(Values.Select(v => v.Value.Copy()).ToList<IVariable>()),
                ValueType.Tuple => this,
                _ => throw new Throw($"Cannot cast tuple as {type.ToString().ToLower()}")
            };
        }

        public override string ToString(int depth)
        {
            if (!Values.Any(v => v.Value is Array or Struct or Tuple))
                return "(" + string.Join(", ", Values.Select(v => v.Value)) + ")";

            return "(" + string.Join(",\n", Values.Select(v => new string(' ', depth * 4) + v.Value.ToString(depth))) + ")";
        }
    }
}