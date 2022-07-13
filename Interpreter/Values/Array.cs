using System.Collections.Generic;
using System.Linq;
using Bloc.Interfaces;
using Bloc.Memory;
using Bloc.Pointers;
using Bloc.Results;
using Bloc.Utils;
using Bloc.Variables;

namespace Bloc.Values
{
    public class Array : Value, IIterable, IIndexable
    {
        private bool _assigned;

        public Array() => Values = new();

        public Array(List<IVariable> value) => Values = value;

        public List<IVariable> Values { get; }

        public override ValueType GetType() => ValueType.Array;

        internal override Value Copy()
        {
            return new Array(Values.Select(v => v.Value.Copy()).ToList<IVariable>());
        }

        internal override void Assign()
        {
            _assigned = true;

            for (var i = 0; i < Values.Count; i++)
            {
                Values[i] = new ArrayVariable(Values[i].Value, this);
                Values[i].Value.Assign();
            }
        }

        internal override void Destroy()
        {
            foreach (var value in Values.Cast<Variable>().ToList())
                value.Delete();
        }

        public override bool Equals(Value other)
        {
            if (other is not Array array)
                return false;

            if (Values.Count != array.Values.Count)
                return false;

            for (var i = 0; i < Values.Count; i++)
                if (!Values[i].Value.Equals(array.Values[i].Value))
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

            if (typeof(T) == typeof(Array))
                return (this as T)!;

            throw new Throw($"Cannot implicitly cast array as {typeof(T).Name.ToLower()}");
        }

        public override Value Explicit(ValueType type)
        {
            return type switch
            {
                ValueType.Null => Null.Value,
                ValueType.Bool => Bool.True,
                ValueType.String => new String(ToString()),
                ValueType.Tuple => new Tuple(Values.Select(v => v.Value.Copy()).ToList<IPointer>()),
                ValueType.Array => this,
                _ => throw new Throw($"Cannot cast array as {type.ToString().ToLower()}")
            };
        }

        public override string ToString(int depth)
        {
            if (Values.Count == 0)
                return "{ }";

            if (!Values.Any(v => v.Value is Array or Struct or Tuple))
                return "{ " + string.Join(", ", Values.Select(v => v.Value.ToString())) + " }";

            return "{\n" +
                   string.Join(",\n", Values.Select(v => new string(' ', (depth + 1) * 4) + v.Value.ToString(depth + 1))) + "\n" +
                   new string(' ', depth * 4) + "}";
        }

        public IEnumerable<Value> Iterate()
        {
            foreach (var value in Values)
                yield return value.Value;
        }

        public IPointer Index(Value value, Call call)
        {
            if (value is Number number)
            {
                var index = number.ToInt();

                if (index < 0)
                    index += Values.Count;

                if (index < 0 || index >= Values.Count)
                    throw new Throw("Index out of range");

                var val = Values[index];

                if (!_assigned)
                    return (Value)val;

                var variable = (Variable)val;

                return new VariablePointer(variable);
            }

            if (value is Range range)
            {
                var (start, end) = RangeUtil.GetStartAndEnd(range, Values.Count);

                var values = new List<IVariable>();

                for (int i = start; i != end && end - i > 0 == range.Step > 0; i += range.Step)
                    values.Add(Values[i]);

                if (!_assigned)
                    return new Array(values);

                var variables = values.Cast<Variable>().ToList();

                return new SlicePointer(variables);
            }

            if (value is Function function)
            {
                return new Array(Values.Select(v => function.Invoke(new() { v.Value }, call)).ToList<IVariable>());
            }

            throw new Throw("It should be a number, a range or a function.");
        }
    }
}