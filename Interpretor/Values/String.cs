using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bloc.Interfaces;
using Bloc.Memory;
using Bloc.Results;

namespace Bloc.Values
{
    public class String : Value, IIterable, IIndexable
    {
        public String(string value)
        {
            Value = value;
        }

        public static String Empty { get; } = new("");

        public string Value { get; }

        public override ValueType Type => ValueType.String;

        public IValue Index(Value val, Call _)
        {
            if (val is Number num)
            {
                var index = num.ToInt() >= 0 ? num.ToInt() : Value.Length + num.ToInt();

                if (index < 0 || index >= Value.Length)
                    throw new Throw("Index out of range");

                return new String(Value[index].ToString());
            }

            if (val is Range rng)
            {
                var builder = new StringBuilder();

                var start = (int)(rng.Start != null
                    ? rng.Start >= 0
                        ? rng.Start
                        : Value.Length + rng.Start
                    : rng.Step >= 0
                        ? 0
                        : Value.Length - 1);

                var end = (int)(rng.End != null
                    ? rng.End >= 0
                        ? rng.End
                        : Value.Length + rng.End
                    : rng.Step >= 0
                        ? Value.Length
                        : -1);

                for (var i = start; i != end && end - i > 0 == rng.Step > 0; i += rng.Step)
                    builder.Append(Value[i]);

                return new String(builder.ToString());
            }

            throw new Throw("It should be a number or a range.");
        }

        public IEnumerable<Value> Iterate()
        {
            foreach (var @char in Value)
                yield return new String(@char.ToString());
        }

        public override Value Copy()
        {
            return this;
        }

        public override bool Equals(IValue other)
        {
            if (other.Value is String str)
                return Value == str.Value;

            return false;
        }

        public override T Implicit<T>()
        {
            if (typeof(T) == typeof(Bool))
                return (Bool.True as T)!;

            if (typeof(T) == typeof(String))
                return (this as T)!;

            throw new Throw($"Cannot implicitly cast string as {typeof(T).Name.ToLower()}");
        }

        public override IValue Explicit(ValueType type)
        {
            return type switch
            {
                ValueType.Bool => Bool.True,
                ValueType.String => this,
                ValueType.Tuple => new Tuple(Value.ToCharArray().Select(c => new String(c.ToString())).ToList<IValue>()),
                ValueType.Array => new Array(Value.ToCharArray().Select(c => new String(c.ToString())).ToList<IValue>()),
                _ => throw new Throw($"Cannot cast string as {type.ToString().ToLower()}")
            };
        }

        public override string ToString(int _)
        {
            return $"'{Value}'";
        }
    }
}