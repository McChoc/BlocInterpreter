using System.Collections.Generic;
using System.Linq;
using Bloc.Interfaces;
using Bloc.Memory;
using Bloc.Pointers;
using Bloc.Results;
using Bloc.Utils.Helpers;
using Bloc.Variables;

namespace Bloc.Values
{
    public sealed class Array : Value, IIndexable
    {
        public Array() => Variables = new();

        public Array(List<Value> values)
        {
            Variables = values
                .Select(x => new ArrayVariable(x, this))
                .ToList();
        }

        internal List<ArrayVariable> Variables { get; }

        internal override ValueType GetType() => ValueType.Array;

        internal static Array Construct(List<Value> values)
        {
            return values.Count switch
            {
                0 => new(),
                1 => values[0] switch
                {
                    Null => new(),
                    Number number => new(Enumerable
                        .Repeat((Value)Null.Value, number.GetInt())
                        .ToList()),
                    String @string => new(@string.Value
                        .ToCharArray()
                        .Select(x => (Value)new String(x.ToString()))
                        .ToList()),
                    Array array => array,
                    Struct @struct => new(@struct.Variables
                        .OrderBy(x => x.Key)
                        .Select(x => (Value)new Tuple(new List<Value>()
                        {
                            new String(x.Key),
                            x.Value.Value.Copy()
                        }))
                        .ToList()),
                    Tuple tuple => new(tuple.Variables
                        .Select(x => x.Value.Copy())
                        .ToList()),
                    Iter iter => new(iter
                        .Iterate()
                        .ToList()),
                    Type type => new(type.Value
                        .Select(x => (Value)new Type(x))
                        .ToList()),
                    _ => throw new Throw($"'array' does not have a constructor that takes a '{values[0].GetType().ToString().ToLower()}'")
                },
                2 => (values[0], values[1]) is (var value, Number number)
                    ? new(Enumerable
                        .Repeat(value, number.GetInt())
                        .Select(x => x.Copy())
                        .ToList())
                    : throw new Throw($"'array' does not have a constructor that takes a '{values[0].GetType().ToString().ToLower()}' and a '{values[1].GetType().ToString().ToLower()}'"),
                _ => throw new Throw($"'array' does not have a constructor that takes {values.Count} arguments")
            };
        }

        internal override Value Copy()
        {
            return new Array(Variables
                .Select(x => x.Value.Copy())
                .ToList());
        }

        internal override void Destroy()
        {
            while (Variables.Any())
                Variables.Last().Delete();
        }

        public override string ToString()
        {
            if (Variables.Count == 0)
                return "{ }";

            return "{ " + string.Join(", ", Variables.Select(v => v.Value)) + " }";
        }

        public override int GetHashCode()
        {
            return System.HashCode.Combine(Variables.Count);
        }

        public override bool Equals(object other)
        {
            if (other is not Array array)
                return false;

            if (Variables.Count != array.Variables.Count)
                return false;

            for (var i = 0; i < Variables.Count; i++)
                if (Variables[i].Value != array.Variables[i].Value)
                    return false;

            return true;
        }

        public IValue Index(Value value, Call call)
        {
            if (value is Number number)
            {
                var index = number.GetInt();

                if (index < 0)
                    index += Variables.Count;

                if (index < 0 || index >= Variables.Count)
                    throw new Throw("Index out of range");

                return new VariablePointer(Variables[index]);
            }

            if (value is Range range)
            {
                var (start, end) = RangeUtil.GetStartAndEnd(range, Variables.Count);

                var variables = new List<ArrayVariable>();

                for (int i = start; i != end && end - i > 0 == range.Step > 0; i += range.Step)
                    variables.Add(Variables[i]);

                return new SlicePointer(variables);
            }

            throw new Throw("It should be a number or a range.");
        }
    }
}