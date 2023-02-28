using System.Collections.Generic;
using System.Linq;
using Bloc.Interfaces;
using Bloc.Memory;
using Bloc.Pointers;
using Bloc.Results;
using Bloc.Variables;

namespace Bloc.Values
{
    public sealed class Struct : Value, IIndexable
    {
        public Struct() => Variables = new();

        public Struct(Dictionary<string, Value> values)
        {
            Variables = values
                .ToDictionary(x => x.Key, x => new StructVariable(x.Key, x.Value, this));
        }

        internal Dictionary<string, StructVariable> Variables { get; }

        internal override ValueType GetType() => ValueType.Struct;

        internal static Struct Construct(List<Value> values)
        {
            return values.Count switch
            {
                0 => new(),
                1 => values[0] switch
                {
                    Null => new(),
                    Struct @struct => @struct,
                    _ => throw new Throw($"'struct' does not have a constructor that takes a '{values[0].GetType().ToString().ToLower()}'")
                },
                _ => throw new Throw($"'struct' does not have a constructor that takes {values.Count} arguments")
            };
        }

        internal override Value Copy()
        {
            return new Struct(Variables
                .ToDictionary(x => x.Key, x => x.Value.Value.Copy()));
        }

        internal override void Destroy()
        {
            foreach (var variable in Variables.Values)
                variable.Delete();
        }

        public override string ToString()
        {
            if (Variables.Count == 0)
                return "{ }";

            return "{ " + string.Join(", ", Variables.OrderBy(x => x.Key).Select(x => $"{x.Key} = {x.Value.Value}")) + " }";
        }

        public override int GetHashCode()
        {
            return System.HashCode.Combine(Variables.Count);
        }

        public override bool Equals(object other)
        {
            if (other is not Struct @struct)
                return false;

            if (Variables.Count != @struct.Variables.Count)
                return false;

            foreach (var key in Variables.Keys)
            {
                if (!@struct.Variables.TryGetValue(key, out var value))
                    return false;

                if (Variables[key].Value != value.Value)
                    return false;
            }

            return true;
        }

        public IValue Index(Value index, Call _)
        {
            if (index is not String @string)
                throw new Throw("It should be a string.");

            if (!Variables.ContainsKey(@string.Value))
                throw new Throw($"'{@string.Value}' was not defined inside this struct");

            return new VariablePointer(Variables[@string.Value]);
        }

        public IValue Get(string key)
        {
            if (!Variables.ContainsKey(key))
                throw new Throw($"'{key}' was not defined inside this struct");

            return new VariablePointer(Variables[key]);
        }
    }
}