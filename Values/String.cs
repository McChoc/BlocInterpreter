using CmmInterpretor.Data;
using CmmInterpretor.Results;
using System.Linq;
using System.Text;

namespace CmmInterpretor.Values
{
    public class String : Value, IIterable
    {
        public string Value { get; set; }

        public String() => Value = "";
        public String(string value) => Value = value;

        public IResult Get(Value variable, Engine _)
        {
            if (variable is Number number)
            {
                int index = number.ToInt() >= 0 ? number.ToInt() : Value.Length + number.ToInt();

                return new String (Value[index].ToString());
            }
            else if (variable is Range range)
            {
                var builder = new StringBuilder();

                int start = (int)(range.Start != null
                    ? (range.Start >= 0
                        ? range.Start
                        : Value.Length + range.Start)
                    : (range.Step >= 0
                        ? 0
                        : Value.Length - 1));

                int end = (int)(range.End != null
                    ? (range.End >= 0
                        ? range.End
                        : Value.Length + range.End)
                    : (range.Step >= 0
                        ? Value.Length
                        : -1));

                for (int i = start; i != end && end - i > 0 == range.Step > 0; i += range.Step)
                    builder.Append(Value[i]);

                return new String(builder.ToString());
            }
            else
            {
                return new Throw("It should be a number or a range.");
            }
        }

        public override VariableType TypeOf() => VariableType.String;

        public override Value Copy() => new String(Value);

        public override bool Equals(Value other)
        {
            if (other is String str)
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

        public override IResult Explicit<T>()
        {
            if (typeof(T) == typeof(Bool))
                return Bool.True;

            if (typeof(T) == typeof(String))
                return this;

            if (typeof(T) == typeof(Tuple))
                return new Tuple(Value.ToCharArray().Select(c => new String(c.ToString())).ToList<IValue>());

            if (typeof(T) == typeof(Array))
                return new Array(Value.ToCharArray().Select(c => new String(c.ToString())).ToList<Value>());

            return new Throw($"Cannot cast string as {typeof(T)}");
        }

        public override string ToString(int _) => $"'{Value}'";

        public int Count => Value.Length;
        public Value this[int index] => new String(Value[index].ToString());
    }
}
