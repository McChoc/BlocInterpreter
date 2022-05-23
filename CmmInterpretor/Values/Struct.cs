using CmmInterpretor.Interfaces;
using CmmInterpretor.Memory;
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

        public override ValueType Type => ValueType.Struct;

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

        public override T Implicit<T>()
        {
            if (typeof(T) == typeof(Bool))
                return (Bool.True as T)!;

            if (typeof(T) == typeof(String))
                return (new String(ToString()) as T)!;

            if (typeof(T) == typeof(Struct))
                return (this as T)!;

            throw new Throw($"Cannot implicitly cast struct as {typeof(T).Name.ToLower()}");
        }

        public override IValue Explicit(ValueType type)
        {
            return type switch
            {
                ValueType.Bool => Bool.True,
                ValueType.String => new String(ToString()),
                ValueType.Tuple => new Tuple(Values.OrderBy(x => x.Key).Select(v => v.Value.Copy()).ToList<IValue>()),
                ValueType.Array => new Array(Values.OrderBy(x => x.Key).Select(x => new Struct(new Dictionary<string, IValue>() { { "key", new String(x.Key) }, { "value", x.Value.Copy() } })).ToList<IValue>()),
                ValueType.Struct => this,
                _ => throw new Throw($"Cannot cast struct as {type.ToString().ToLower()}")
            };
        }

        public override string ToString(int depth)
        {
            if (Values.Count == 0)
                return "struct()";
            else
                return "{\n" + string.Join(",\n", Values.OrderBy(x => x.Key).Select(p => new string(' ', (depth + 1) * 4) + p.Key + " = " + p.Value.ToString(depth + 1))) + "\n" + new string(' ', depth * 4) + "}";
        }

        public IValue Get(string identifier)
        {
            if (Values.ContainsKey(identifier))
                return Values[identifier];

            throw new Throw($"'{identifier}' was not defined inside this struct");
        }

        public IValue Index(Value value, Call _)
        {
            if (value is not String str)
                throw new Throw("It should be a string.");

            if (Values.ContainsKey(str.Value))
                return Values[str.Value];

            throw new Throw($"'{str.Value}' was not defined inside this struct");
        }
    }
}
