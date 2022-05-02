using CmmInterpretor.Data;
using CmmInterpretor.Results;
using CmmInterpretor.Variables;
using System.Collections.Generic;
using System.Linq;

namespace CmmInterpretor.Values
{
    public class Tuple : Value
    {
        public List<IValue> Values { get; }

        public override VariableType Type => VariableType.Tuple;

        public Tuple(List<IValue> value) => Values = value;

        public override Value Copy() => new Tuple(Values.Select(v => v.Copy()).ToList<IValue>());
        public override void Assign()
        {
            for (int i = 0; i < Values.Count; i++)
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

            for (int i = 0; i < Values.Count; i++)
                if (!Values[i].Equals(tpl.Values[i]))
                    return false;

            return true;
        }

        public override bool Implicit<T>(out T value)
        {
            if (typeof(T) == typeof(Bool))
            {
                value = Bool.True as T;

                foreach (var variable in Values)
                {
                    if (variable.Implicit(out Bool b))
                    {
                        if (!b.Value)
                            value = Bool.False as T;
                    }
                    else
                    {
                        value = null;
                        return false;
                    }
                }

                return true;
            }

            if (typeof(T) == typeof(String))
            {
                value = new String(ToString()) as T;
                return true;
            }

            if (typeof(T) == typeof(Tuple))
            {
                value = this as T;
                return true;
            }

            value = null;
            return false;
        }

        public override IResult Implicit(VariableType type)
        {
            if (type == VariableType.Bool)
            {
                foreach (var variable in Values)
                {
                    var result = variable.Implicit(VariableType.Bool);

                    if (result is not IValue v || !((Bool)v.Value).Value)
                        return result;
                }

                return Bool.True;
            }

            return type switch
            {
                VariableType.String => new String(ToString()),
                VariableType.Tuple => this,
                _ => new Throw($"Cannot implicitly cast tuple as {type.ToString().ToLower()}")
            };
        }

        public override IResult Explicit(VariableType type)
        {
            if (type == VariableType.Bool)
            {
                foreach (var variable in Values)
                {
                    var result = variable.Explicit(VariableType.Bool);

                    if (result is not IValue v || !((Bool)v.Value).Value)
                        return result;
                }

                return Bool.True;
            }

            return type switch
            {
                VariableType.String => new String(ToString()),
                VariableType.Array => new Array(Values.Select(v => v.Copy()).ToList<IValue>()),
                VariableType.Tuple => this,
                _ => new Throw($"Cannot cast tuple as {type.ToString().ToLower()}")
            };
        }

        public override string ToString(int depth)
        {
            if (!Values.Any(v => v.Value is Array or Struct or Tuple))
                return "(" + string.Join(", ", Values.Select(v => v.Value)) + ")";
            else
                return "(\n" + string.Join(",\n", Values.Select(v => new string(' ', (depth + 1) * 4) + v.Value.ToString(depth + 1))) + "\n" + new string(' ', depth * 4) + ")";
        }
    }
}
