using CmmInterpretor.Data;
using CmmInterpretor.Results;
using System.Collections.Generic;
using System.Linq;

namespace CmmInterpretor.Values
{
    public class Tuple : Value
    {
        public List<IValue> Variables { get; set; }

        public Tuple() => Variables = new List<IValue>();
        public Tuple(List<IValue> value) => Variables = value;

        public override VariableType TypeOf() => VariableType.Tuple;

        public override Value Copy() => new Tuple(Variables.Select(v => v.Value().Copy()).ToList<IValue>());

        public override bool Equals(Value other)
        {
            if (other is not Tuple tpl)
                return false;

            if (Variables.Count != tpl.Variables.Count)
                return false;

            for (int i = 0; i < Variables.Count; i++)
                if (!Variables[i].Equals(tpl.Variables[i]))
                    return false;

            return true;
        }

        public override bool Implicit<T>(out T value)
        {
            if (typeof(T) == typeof(Bool))
            {
                value = Bool.True as T;

                foreach (var variable in Variables)
                {
                    if (variable.Value().Implicit(out Bool b))
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

        public override IResult Explicit<T>()
        {
            if (typeof(T) == typeof(Bool))
            {
                foreach (var variable in Variables)
                {
                    var result = variable.Value().Explicit<Bool>();

                    if (result is not IValue v || !((Bool)v.Value()).Value)
                        return result;
                }

                return Bool.True;
            }

            if (typeof(T) == typeof(String))
                return new String(ToString());

            if (typeof(T) == typeof(Array))
                return new Array(Variables.Select(v => v.Value().Copy()).ToList());

            if (typeof(T) == typeof(Tuple))
                return this;

            return new Throw($"Cannot cast tuple as {typeof(T)}");
        }

        public override string ToString(int depth)
        {
            if (!Variables.Any(v => v is Tuple || v is Array || v is Struct))
                return "(" + string.Join(", ", Variables.Select(v => v.Value())) + ")";
            else
                return "(\n" + string.Join(",\n", Variables.Select(v => new string(' ', (depth + 1) * 4) + v.Value().ToString(depth + 1))) + "\n" + new string(' ', depth * 4) + ")";
        }
    }
}
