using System.Collections.Generic;
using System.Linq;
using Bloc.Pointers;
using Bloc.Results;

namespace Bloc.Values
{
    public sealed class Tuple : Value
    {
        public Tuple() => Values = new();

        public Tuple(List<IPointer> value) => Values = value;

        public List<IPointer> Values { get; }

        internal override ValueType GetType() => ValueType.Tuple;

        internal override Value Copy()
        {
            return new Tuple(Values.Select(v => v.Value.Copy()).ToList<IPointer>());
        }

        internal override void Assign()
        {
            foreach (var value in Values)
                value.Value.Assign();
        }

        public override bool Equals(Value other)
        {
            if (other is not Tuple tuple)
                return false;

            if (Values.Count != tuple.Values.Count)
                return false;

            for (var i = 0; i < Values.Count; i++)
                if (!Values[i].Value.Equals(tuple.Values[i].Value))
                    return false;

            return true;
        }

        internal static Tuple Construct(List<Value> values)
        {
            return values.Count switch
            {
                0 => new(),
                1 => values[0] switch
                {
                    Null => new(),
                    Range range => new(),
                    Array array => new(array.Values
                        .Select(x => x.Value.Copy())
                        .ToList<IPointer>()),
                    Struct @struct => new(@struct.Values
                        .OrderBy(x => x.Key)
                        .Select(x => x.Value.Value.Copy())
                        .ToList<IPointer>()),
                    Tuple tuple => tuple,
                    _ => throw new Throw($"'iter' does not have a constructor that takes a '{values[0].GetType().ToString().ToLower()}'")
                },
                _ => throw new Throw($"'iter' does not have a constructor that takes {values.Count} arguments")
            };
        }

        public override string ToString(int depth)
        {
            if (!Values.Any(v => v.Value is Array or Struct or Tuple))
                return "(" + string.Join(", ", Values.Select(v => v.Value)) + ")";

            return "(" + string.Join(",\n", Values.Select(v => new string(' ', depth * 4) + v.Value.ToString(depth))) + ")";
        }
    }
}