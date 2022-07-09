using System.Collections.Generic;
using System.Linq;
using Bloc.Interfaces;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Variables;

namespace Bloc.Values
{
    public class Array : Value, IIterable, IIndexable
    {
        public Array(List<IValue> value)
        {
            Values = value;
        }

        public static Array Empty { get; } = new(new());

        public List<IValue> Values { get; }

        public override ValueType GetType() => ValueType.Array;

        public IValue Index(Value val, Call call)
        {
            if (val is Number num)
            {
                var index = num.ToInt() >= 0 ? num.ToInt() : Values.Count + num.ToInt();

                if (index >= 0 && index < Values.Count)
                    return Values[index];

                throw new Throw("Index out of range");
            }

            if (val is Range rng)
            {
                var variables = new List<IValue>();

                var start = (int)(rng.Start != null
                    ? rng.Start >= 0
                        ? rng.Start
                        : Values.Count + rng.Start
                    : rng.Step >= 0
                        ? 0
                        : Values.Count - 1);

                var end = (int)(rng.End != null
                    ? rng.End >= 0
                        ? rng.End
                        : Values.Count + rng.End
                    : rng.Step >= 0
                        ? Values.Count
                        : -1);

                for (var i = start; i != end && end - i > 0 == rng.Step > 0; i += rng.Step)
                    variables.Add(Values[i].Value);

                return new Array(variables);
            }

            if (val is Function func)
            {
                var list = new List<IValue>();

                foreach (var value in Values)
                {
                    var result = func.Invoke(new List<Value> { value.Value }, call);

                    if (result is IValue v)
                        list.Add(v.Value);
                    else
                        return result;
                }

                return new Array(list);
            }

            throw new Throw("It should be a number, a range or a function.");
        }

        public IEnumerable<Value> Iterate()
        {
            foreach (var value in Values)
                yield return value.Value;
        }

        public override Value Copy()
        {
            return new Array(Values.Select(v => v.Value.Copy()).ToList<IValue>());
        }

        public override void Assign()
        {
            for (var i = 0; i < Values.Count; i++)
            {
                Values[i] = new ChildVariable((Value)Values[i], i, this);
                Values[i].Value.Assign();
            }
        }

        public override void Destroy()
        {
            foreach (var value in Values)
                value.Value.Destroy();
        }

        public override bool Equals(IValue other)
        {
            if (other.Value is not Array arr)
                return false;

            if (Values.Count != arr.Values.Count)
                return false;

            for (var i = 0; i < Values.Count; i++)
                if (!Values[i].Value.Equals(arr.Values[i]))
                    return false;

            return true;
        }

        public override T Implicit<T>()
        {
            if (typeof(T) == typeof(Bool))
                return (Bool.True as T)!;

            if (typeof(T) == typeof(String))
                return (new String(ToString()) as T)!;

            if (typeof(T) == typeof(Array))
                return (this as T)!;

            throw new Throw($"Cannot implicitly cast array as {typeof(T).Name.ToLower()}");
        }

        public override IValue Explicit(ValueType type)
        {
            return type switch
            {
                ValueType.Bool => Bool.True,
                ValueType.String => new String(ToString()),
                ValueType.Tuple => new Tuple(Values.Select(v => v.Value.Copy()).ToList<IValue>()),
                ValueType.Array => this,
                _ => throw new Throw($"Cannot cast array as {type.ToString().ToLower()}")
            };
        }

        public override string ToString(int depth)
        {
            if (Values.Count == 0)
                return "array()";

            if (!Values.Any(v => v.Value is Array or Struct or Tuple))
                return "{ " + string.Join(", ", Values.Select(v => v.Value.ToString())) + " }";

            return "{\n" +
                   string.Join(",\n", Values.Select(v => new string(' ', (depth + 1) * 4) + v.Value.ToString(depth + 1))) + "\n" +
                   new string(' ', depth * 4) + "}";
        }
    }
}