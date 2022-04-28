using CmmInterpretor.Data;
using CmmInterpretor.Results;
using System.Collections.Generic;
using System.Linq;

namespace CmmInterpretor.Values
{
    public class Struct : Value
    {
        public Dictionary<string, Value> Variables { get; set; }

        public Struct() => Variables = new Dictionary<string, Value>();
        public Struct(Dictionary<string, Value> value) => Variables = value;

        public IResult Get(Value variable, Engine _)
        {
            if (variable is String str)
                return Variables[str.Value];
            else
                return new Throw("It should be a string.");
        }

        public override VariableType TypeOf() => VariableType.Struct;

        public override Value Copy() => new Struct(Variables.ToDictionary(p => p.Key, p => p.Value.Copy()));

        public override bool Equals(Value other)
        {
            if (other is not Struct obj)
                return false;

            if (Variables.Count != obj.Variables.Count)
                return false;

            foreach (string key in Variables.Keys)
                if (!Variables[key].Equals(obj.Variables[key]))
                    return false;

            return true;
        }

        public override bool Implicit<T>(out T value)
        {
            if (typeof(T) == typeof(Bool))
            {
                value = Bool.True as T;
                return true;
            }

            if (typeof(T) == typeof(String))
            {
                value = new String(ToString()) as T;
                return true;
            }

            if (typeof(T) == typeof(Struct))
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
                return Bool.True;

            if (typeof(T) == typeof(String))
                return new String(ToString());

            if (typeof(T) == typeof(Tuple))
                return new Tuple(Variables.OrderBy(x => x.Key).Select(v => v.Value.Copy()).ToList<IValue>());

            if (typeof(T) == typeof(Array))
                return new Array(Variables.OrderBy(x => x.Key).Select(x => new Struct(new Dictionary<string, Value>() { { "key", new String(x.Key) }, { "value", x.Value.Copy() } })).ToList<Value>());

            if (typeof(T) == typeof(Struct))
                return this;

            return new Throw($"Cannot cast struct as {typeof(T)}");
        }

        public override string ToString(int depth)
        {
            if (Variables.Count == 0)
                return "{ } as struct";
            else
                return "{\n" + string.Join(",\n", Variables.OrderBy(x => x.Key).Select(p => new string(' ', (depth + 1) * 4) + p.Key + " = " + p.Value.ToString(depth + 1))) + "\n" + new string(' ', depth * 4) + "}";
        }
    }
}
