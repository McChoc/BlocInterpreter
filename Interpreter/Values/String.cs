using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bloc.Interfaces;
using Bloc.Memory;
using Bloc.Pointers;
using Bloc.Results;
using Bloc.Utils;
using Bloc.Variables;

namespace Bloc.Values
{
    public class String : Value, IIterable, IIndexable
    {
        public String(string value) => Value = value;

        public static String Empty { get; } = new("");

        public string Value { get; }

        public override ValueType GetType() => ValueType.String;

        public override bool Equals(Value other)
        {
            if (other is String @string)
                return Value == @string.Value;

            return false;
        }

        public override T Implicit<T>()
        {
            if (typeof(T) == typeof(Null))
                return (Null.Value as T)!;

            if (typeof(T) == typeof(Bool))
                return (Bool.True as T)!;

            if (typeof(T) == typeof(String))
                return (this as T)!;

            throw new Throw($"Cannot implicitly cast string as {typeof(T).Name.ToLower()}");
        }

        public override Value Explicit(ValueType type)
        {
            return type switch
            {
                ValueType.Null => Null.Value,
                ValueType.Bool => Bool.True,
                ValueType.String => this,
                ValueType.Tuple => new Tuple(Value.ToCharArray().Select(c => new String(c.ToString())).ToList<IPointer>()),
                ValueType.Array => new Array(Value.ToCharArray().Select(c => new String(c.ToString())).ToList<IVariable>()),
                _ => throw new Throw($"Cannot cast string as {type.ToString().ToLower()}")
            };
        }

        public override string ToString(int _)
        {
            return $"\"{Value}\"";
        }

        public IEnumerable<Value> Iterate()
        {
            foreach (var @char in Value)
                yield return new String(@char.ToString());
        }

        public IPointer Index(Value value, Call _)
        {
            if (value is Number number)
            {
                var index = number.ToInt();

                if (index < 0)
                    index += Value.Length;

                if (index < 0 || index >= Value.Length)
                    throw new Throw("Index out of range");

                return new String(Value[index].ToString());
            }

            if (value is Range range)
            {
                var (start, end) = RangeUtil.GetStartAndEnd(range, Value.Length);

                var builder = new StringBuilder();

                for (int i = start; i != end && end - i > 0 == range.Step > 0; i += range.Step)
                    builder.Append(Value[i]);

                return new String(builder.ToString());
            }

            throw new Throw("It should be a number or a range.");
        }
    }
}