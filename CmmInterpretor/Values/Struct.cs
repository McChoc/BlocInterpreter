using CmmInterpretor.Data;
using CmmInterpretor.Results;
using CmmInterpretor.Variables;
using System.Collections.Generic;
using System.Linq;

namespace CmmInterpretor.Values
{
    public class Struct : Value, IIndexable
    {
        public static Struct Empty { get; } = new(new());

        public Dictionary<string, IValue> Values { get; private set; }

        public override VariableType Type => VariableType.Struct;

        public Struct(Dictionary<string, IValue> value) => Values = value;

        public override Value Copy() => new Struct(Values.ToDictionary(p => p.Key, p => (IValue)p.Value.Copy()));
        public override void Assign()
        {
            foreach (var key in Values.Keys)
            {
                Values[key] = new ChildVariable((Value)Values[key], key, this);
                Values[key].Assign();
            }
        }
        public override void Destroy()
        {
            foreach (var value in Values.Values)
                value.Destroy();
        }

        public override bool Equals(IValue other)
        {
            if (other.Value is not Struct obj)
                return false;

            if (Values.Count != obj.Values.Count)
                return false;

            foreach (string key in Values.Keys)
                if (!Values[key].Equals(obj.Values[key]))
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

        public override IResult Implicit(VariableType type)
        {
            return type switch
            {
                VariableType.Bool => Bool.True,
                VariableType.String => new String(ToString()),
                VariableType.Struct => this,
                _ => new Throw($"Cannot implicitly cast struct as {type.ToString().ToLower()}")
            };
        }

        public override IResult Explicit(VariableType type)
        {
            return type switch
            {
                VariableType.Bool => Bool.True,
                VariableType.String => new String(ToString()),
                VariableType.Tuple => new Tuple(Values.OrderBy(x => x.Key).Select(v => v.Value.Copy()).ToList<IValue>()),
                VariableType.Array => new Array(Values.OrderBy(x => x.Key).Select(x => new Struct(new Dictionary<string, IValue>() { { "key", new String(x.Key) }, { "value", x.Value.Copy() } })).ToList<IValue>()),
                VariableType.Struct => this,
                _ => new Throw($"Cannot cast struct as {type.ToString().ToLower()}")
            };
        }

        public override string ToString(int depth)
        {
            if (Values.Count == 0)
                return "{ } as struct";
            else
                return "{\n" + string.Join(",\n", Values.OrderBy(x => x.Key).Select(p => new string(' ', (depth + 1) * 4) + p.Key + " = " + p.Value.ToString(depth + 1))) + "\n" + new string(' ', depth * 4) + "}";
        }

        public IResult Get(string identifier)
        {
            if (Values.ContainsKey(identifier))
                return (IResult)Values[identifier];

            return new Throw($"'{identifier}' was not defined inside this struct");
        }

        public IResult Index(Value value, Engine engine)
        {
            if (value is not String str)
                return new Throw("It should be a string.");

            if (Values.ContainsKey(str.Value))
                return (IResult)Values[str.Value];

            return new Throw($"'{str.Value}' was not defined inside this struct");
        }
    }
}
