using CmmInterpretor.Data;
using CmmInterpretor.Results;
using CmmInterpretor.Variables;
using System.Collections.Generic;
using System.Linq;

namespace CmmInterpretor.Values
{
    public class Array : Value, IIterable, IIndexable
    {
        public static Array Empty { get; } = new(new());

        public List<IValue> Values { get; private set; }

        public override VariableType Type => VariableType.Array;

        public Array(List<IValue> value) => Values = value;

        public override Value Copy() => new Array(Values.Select(v => v.Copy()).ToList<IValue>());
        public override void Assign()
        {
            for (int i = 0; i < Values.Count; i++)
            {
                Values[i] = new ChildVariable((Value)Values[i], i, this);
                Values[i].Assign();
            }
        }
        public override void Destroy()
        {
            foreach (var value in Values)
                value.Destroy();
        }

        public override bool Equals(IValue other)
        {
            if (other.Value is not Array arr)
                return false;

            if (Values.Count != arr.Values.Count)
                return false;

            for (int i = 0; i < Values.Count; i++)
                if (!Values[i].Equals(arr.Values[i]))
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

        public override IResult Implicit(VariableType type)
        {
            return type switch
            {
                VariableType.Bool => Bool.True,
                VariableType.String => new String(ToString()),
                VariableType.Array => this,
                _ => new Throw($"Cannot implicitly cast array as {type.ToString().ToLower()}")
            };
        }

        public override IResult Explicit(VariableType type)
        {
            return type switch
            {
                VariableType.Bool => Bool.True,
                VariableType.String => new String(ToString()),
                VariableType.Tuple => new Tuple(Values.Select(v => v.Copy()).ToList<IValue>()),
                VariableType.Array => this,
                _ => new Throw($"Cannot cast array as {type.ToString().ToLower()}")
            };
        }

        public override string ToString(int depth)
        {
            if (Values.Count == 0)
                return "{ } as array";
            else if (!Values.Any(v => v.Value is Array or Struct or Tuple))
                return "{ " + string.Join(", ", Values.Select(v => v.Value.ToString())) + " }";
            else
                return "{\n" + string.Join(",\n", Values.Select(v => new string(' ', (depth + 1) * 4) + v.ToString(depth + 1))) + "\n" + new string(' ', depth * 4) + "}";
        }

        public IEnumerable<Value> Iterate()
        {
            foreach (var value in Values)
                yield return value.Value;
        }

        public IResult Index(Value val, Engine engine)
        {
            if (val is Number num)
            {
                int index = num.ToInt() >= 0 ? num.ToInt() : Values.Count + num.ToInt();

                if (index >= 0 && index < Values.Count)
                    return (IResult)Values[index];

                return new Throw("Index out of range");
            }
            
            if (val is Range rng)
            {
                var variables = new List<IValue>();

                int start = (int)(rng.Start != null
                    ? (rng.Start >= 0
                        ? rng.Start
                        : Values.Count + rng.Start)
                    : (rng.Step >= 0
                        ? 0
                        : Values.Count - 1));

                int end = (int)(rng.End != null
                    ? (rng.End >= 0
                        ? rng.End
                        : Values.Count + rng.End)
                    : (rng.Step >= 0
                        ? Values.Count
                        : -1));

                for (int i = start; i != end && end - i > 0 == rng.Step > 0; i += rng.Step)
                    variables.Add(Values[i].Value);

                return new Array(variables);
            }
            
            if (val is Function func)
            {
                var list = new List<IValue>();

                foreach (var value in Values)
                {
                    var result = func.Call(new Array(new List<IValue>() { value }), engine);

                    if (result is IValue v)
                        list.Add(v.Value);
                    else
                        return result;
                }

                return new Array(list);
            }

            return new Throw("It should be a number, a range or a function.");
        }
    }
}
