using CmmInterpretor.Data;
using CmmInterpretor.Results;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CmmInterpretor.Values
{
    public class String : Value, IIterable, IIndexable
    {
        public static String Empty { get; } = new("");

        public string Value { get; }

        public override VariableType Type => VariableType.String;

        public String(string value) => Value = value;

        public override Value Copy() => this;

        public override bool Equals(IValue other)
        {
            if (other.Value is String str)
                return Value == str.Value;

            return false;
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
                VariableType.String => this,
                _ => new Throw($"Cannot implicitly cast string as {type.ToString().ToLower()}")
            };
        }

        public override IResult Explicit(VariableType type)
        {
            return type switch
            {
                VariableType.Bool => Bool.True,
                VariableType.String => this,
                VariableType.Tuple => new Tuple(Value.ToCharArray().Select(c => new String(c.ToString())).ToList<IValue>()),
                VariableType.Array => new Array(Value.ToCharArray().Select(c => new String(c.ToString())).ToList<IValue>()),
                _ => new Throw($"Cannot cast string as {type.ToString().ToLower()}")
            };
        }

        public override string ToString(int _) => $"'{Value}'";

        public IEnumerable<Value> Iterate()
        {
            foreach (var @char in Value)
                yield return new String(@char.ToString());
        }

        public IResult Index(Value val, Engine _)
        {
            if (val is Number num)
            {
                int index = num.ToInt() >= 0 ? num.ToInt() : Value.Length + num.ToInt();

                return new String(Value[index].ToString());
            }

            if (val is Range rng)
            {
                var builder = new StringBuilder();

                int start = (int)(rng.Start != null
                    ? (rng.Start >= 0
                        ? rng.Start
                        : Value.Length + rng.Start)
                    : (rng.Step >= 0
                        ? 0
                        : Value.Length - 1));

                int end = (int)(rng.End != null
                    ? (rng.End >= 0
                        ? rng.End
                        : Value.Length + rng.End)
                    : (rng.Step >= 0
                        ? Value.Length
                        : -1));

                for (int i = start; i != end && end - i > 0 == rng.Step > 0; i += rng.Step)
                    builder.Append(Value[i]);

                return new String(builder.ToString());
            }
            
            return new Throw("It should be a number or a range.");
        }
    }
}
