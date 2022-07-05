using System.Collections.Generic;
using System.Linq;
using Bloc.Results;
using Bloc.Variables;

namespace Bloc.Values
{
    public class Tuple : Value
    {
        public Tuple(List<IValue> value)
        {
            Values = value;
        }

        public List<IValue> Values { get; }

        public override ValueType Type => ValueType.Tuple;

        public override Value Copy()
        {
            return new Tuple(Values.Select(v => v.Copy()).ToList<IValue>());
        }

        public override void Assign()
        {
            for (var i = 0; i < Values.Count; i++)
            {
                Values[i] = new ChildVariable(Values[i].Value, null, this);
                Values[i].Assign();
            }
        }

        public override bool Equals(IValue other)
        {
            if (other.Value is not Tuple tpl)
                return false;

            if (Values.Count != tpl.Values.Count)
                return false;

            for (var i = 0; i < Values.Count; i++)
                if (!Values[i].Equals(tpl.Values[i]))
                    return false;

            return true;
        }

        public override T Implicit<T>()
        {
            if (typeof(T) == typeof(Bool))
            {
                foreach (var variable in Values)
                    if (!variable.Implicit<Bool>().Value)
                        return (Bool.False as T)!;

                return (Bool.True as T)!;
            }

            if (typeof(T) == typeof(String))
                return (new String(ToString()) as T)!;

            if (typeof(T) == typeof(Tuple))
                return (this as T)!;

            throw new Throw($"Cannot implicitly cast tuple as {typeof(T).Name.ToLower()}");
        }

        public override IValue Explicit(ValueType type)
        {
            if (type == ValueType.Bool)
            {
                foreach (var variable in Values)
                {
                    var result = variable.Explicit(ValueType.Bool);

                    if (result is not IValue v || !((Bool)v.Value).Value)
                        return result;
                }

                return Bool.True;
            }

            return type switch
            {
                ValueType.String => new String(ToString()),
                ValueType.Array => new Array(Values.Select(v => v.Copy()).ToList<IValue>()),
                ValueType.Tuple => this,
                _ => throw new Throw($"Cannot cast tuple as {type.ToString().ToLower()}")
            };
        }

        public override string ToString(int depth)
        {
            if (!Values.Any(v => v.Value is Array or Struct or Tuple))
                return "(" + string.Join(", ", Values.Select(v => v.Value)) + ")";

            return "(\n" +
                   string.Join(",\n", Values.Select(v => new string(' ', (depth + 1) * 4) + v.Value.ToString(depth + 1))) + "\n" +
                   new string(' ', depth * 4) + ")";
        }
    }
}