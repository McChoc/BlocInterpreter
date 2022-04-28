using CmmInterpretor.Data;
using CmmInterpretor.Results;
using System.Collections.Generic;
using System.Linq;

namespace CmmInterpretor.Values
{
    public class Array : Value, IIterable
    {
        public List<Value> Variables { get; set; }

        public Array() => Variables = new();
        public Array(List<Value> value) => Variables = value;

        public IResult Get(Value variable, Engine engine)
        {
            if (variable is Number number)
            {
                int index = number.ToInt() >= 0 ? number.ToInt() : Variables.Count + number.ToInt();

                return Variables[index];
            }
            else if (variable is Range range)
            {
                var variables = new List<Value>();

                int start = (int)(range.Start != null
                    ? (range.Start >= 0
                        ? range.Start
                        : Variables.Count + range.Start)
                    : (range.Step >= 0
                        ? 0
                        : Variables.Count - 1));

                int end = (int)(range.End != null
                    ? (range.End >= 0
                        ? range.End
                        : Variables.Count + range.End)
                    : (range.Step >= 0
                        ? Variables.Count
                        : -1));

                for (int i = start; i != end && end - i > 0 == range.Step > 0; i += range.Step)
                    variables.Add(Variables[i]);

                return new Array(variables);
            }
            else if (variable is Function function)
            {
                var list = new List<Value>();

                foreach (var v in Variables)
                {
                    var result = function.Call(new Array(new List<Value>() { v }), engine);

                    if (result is IValue value)
                        list.Add(value.Value());
                    else
                        return result;
                }

                return new Array(list);
            }
            else
            {
                return new Throw("It should be a number, a range or a function.");
            }
        }

        public override VariableType TypeOf() => VariableType.Array;

        public override Value Copy() => new Array(Variables.Select(v => v.Copy()).ToList());

        public override bool Equals(Value other)
        {
            if (other is not Array arr)
                return false;

            if (Variables.Count != arr.Variables.Count)
                return false;

            for (int i = 0; i < Variables.Count; i++)
                if (!Variables[i].Equals(arr.Variables[i]))
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

            if (typeof(T) == typeof(Array))
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
                return new Tuple(Variables.Select(v => v.Copy()).ToList<IValue>());

            if (typeof(T) == typeof(Array))
                return this;

            return new Throw($"Cannot cast array as {typeof(T)}");
        }

        public override string ToString(int depth)
        {
            if (Variables.Count == 0)
                return "{ } as array";
            else if (!Variables.Any(v => v is Array || v is Struct))
                return "{ " + string.Join(", ", Variables) + " }";
            else
                return "{\n" + string.Join(",\n", Variables.Select(v => new string(' ', (depth + 1) * 4) + v.ToString(depth + 1))) + "\n" + new string(' ', depth * 4) + "}";
        }

        public int Count => Variables.Count;

        public Value this[int index] => Variables[index];
    }
}
