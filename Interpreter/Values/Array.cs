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
    public sealed class Array : Value, IIndexable
    {
        private bool _assigned;

        public Array() => Values = new();

        public Array(List<IVariable> value) => Values = value;

        public List<IVariable> Values { get; }

        internal override ValueType GetType() => ValueType.Array;

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

        internal static Array Construct(List<Value> values)
        {
            return values.Count switch
            {
                0 => new(),
                1 => values[0] switch
                {
                    Null => new(),
                    Number number => new(Enumerable
                        .Repeat(Null.Value, number.GetInt())
                        .ToList<IVariable>()),
                    String @string => new(@string.Value
                        .ToCharArray()
                        .Select(x => new String(x.ToString()))
                        .ToList<IVariable>()),
                    Array array => array,
                    Struct @struct => new(@struct.Values
                        .OrderBy(x => x.Key)
                        .Select(x => new Tuple(new()
                        {
                            new String(x.Key),
                            x.Value.Value.Copy()
                        }))
                        .ToList<IVariable>()),
                    Tuple tuple => new(tuple.Values
                        .Select(x => x.Value.Copy())
                        .ToList<IVariable>()),
                    Iter iter => new(iter
                        .Iterate()
                        .ToList<IVariable>()),
                    Type type => new(type.Value
                        .Select(x => new Type(x))
                        .ToList<IVariable>()),
                    var value => throw new Throw($"'array' does not have a constructor that takes a '{value.GetType().ToString().ToLower()}'")
                },
                2 => (values[0], values[1]) is (var value, Number number)
                    ? new(Enumerable
                        .Repeat(value, number.GetInt())
                        .Select(x => x.Copy())
                        .ToList<IVariable>())
                    : throw new Throw($"'array' does not have a constructor that takes a '{values[0].GetType().ToString().ToLower()}' and a '{values[1].GetType().ToString().ToLower()}'"),
                _ => throw new Throw($"'array' does not have a constructor that takes {values.Count} arguments")
            };
        }

        public IPointer Index(Value value, Call call)
        {
            if (value is Number number)
            {
                var index = number.GetInt();

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

            if (value is Func func)
            {
                return new Array(Values.Select(v => func.Invoke(new() { v.Value }, call)).ToList<IVariable>());
            }

            throw new Throw("It should be a number or a range.");
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
    }
}