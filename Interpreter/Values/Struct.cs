using System.Collections.Generic;
using System.Linq;
using Bloc.Interfaces;
using Bloc.Memory;
using Bloc.Pointers;
using Bloc.Results;
using Bloc.Variables;

namespace Bloc.Values
{
    public class Struct : Value, IIndexable
    {
        private bool _assigned;

        public Struct() => Values = new();

        public Struct(Dictionary<string, IVariable> value) => Values = value;

        public Dictionary<string, IVariable> Values { get; }

        public override ValueType GetType() => ValueType.Struct;

        internal override Value Copy()
        {
            return new Struct(Values.ToDictionary(p => p.Key, p => (IVariable)p.Value.Value.Copy()));
        }

        internal override void Assign()
        {
            _assigned = true;

            foreach (var key in Values.Keys)
            {
                Values[key] = new StructVariable(key, Values[key].Value, this);
                Values[key].Value.Assign();
            }
        }

        internal override void Destroy()
        {
            foreach (var value in Values.Values.Cast<StructVariable>())
                value.Delete();
        }

        public override bool Equals(Value other)
        {
            if (other is not Struct @struct)
                return false;

            if (Values.Count != @struct.Values.Count)
                return false;

            foreach (var key in Values.Keys)
                if (!Values[key].Value.Equals(@struct.Values[key].Value))
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

            if (typeof(T) == typeof(Struct))
                return (this as T)!;

            throw new Throw($"Cannot implicitly cast struct as {typeof(T).Name.ToLower()}");
        }

        public override Value Explicit(ValueType type)
        {
            return type switch
            {
                ValueType.Null => Null.Value,
                ValueType.Bool => Bool.True,
                ValueType.String => new String(ToString()),
                ValueType.Array => new Array(Values.OrderBy(x => x.Key).Select(x =>
                    new Struct(new() {
                        { "key", new String(x.Key) },
                        { "value", x.Value.Value.Copy() }
                    })).ToList<IVariable>()),
                ValueType.Struct => this,
                ValueType.Tuple => new Tuple(Values.OrderBy(x => x.Key).Select(x => x.Value.Value.Copy())
                    .ToList<IPointer>()),
                _ => throw new Throw($"Cannot cast struct as {type.ToString().ToLower()}")
            };
        }

        public override string ToString(int depth)
        {
            if (Values.Count == 0)
                return "{ }";

            return "{\n" +
                   string.Join(",\n", Values.OrderBy(x => x.Key).Select(p => new string(' ', (depth + 1) * 4) + p.Key + " = " + p.Value.Value.ToString(depth + 1))) + "\n" +
                   new string(' ', depth * 4) + "}";
        }

        public IPointer Index(Value index, Call _)
        {
            if (index is not String str)
                throw new Throw("It should be a string.");

            if (!Values.ContainsKey(str.Value))
                throw new Throw($"'{str.Value}' was not defined inside this struct");

            var value = Values[str.Value];

            if (!_assigned)
                return (Value)value;

            var variable = (Variable)value;

            return new VariablePointer(variable);
        }

        public IPointer Get(string key)
        {
            if (!Values.ContainsKey(key))
                throw new Throw($"'{key}' was not defined inside this struct");

            var value = Values[key];

            if (!_assigned)
                return (Value)value;

            var variable = (Variable)value;

            return new VariablePointer(variable);
        }
    }
}