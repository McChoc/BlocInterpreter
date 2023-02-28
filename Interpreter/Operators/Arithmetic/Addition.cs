using System.Collections.Generic;
using Bloc.Expressions;
using Bloc.Interfaces;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Utils.Helpers;
using Bloc.Values;

namespace Bloc.Operators
{
    internal sealed record Addition : IExpression
    {
        private readonly IExpression _left;
        private readonly IExpression _right;

        internal Addition(IExpression left, IExpression right)
        {
            _left = left;
            _right = right;
        }

        public IValue Evaluate(Call call)
        {
            var left = _left.Evaluate(call).Value;
            var right = _right.Evaluate(call).Value;

            return OperatorUtil.RecursivelyCall(left, right, Operation, call);
        }

        internal static Value Operation(Value a, Value b)
        {
            return (a, b) switch
            {
                (IScalar left, IScalar right)   => AddScalars(left, right),
                (String left, String right)     => ConcatStrings(left, right),
                (Array left, Array right)       => ConcatArrays(left, right),
                (Struct left, Struct right)     => MergeStructs(left, right),
                (Value value, Array array)      => PrependToArray(array, value),
                (Array array, Value value)      => AppendToArray(array, value),
                (Value value, String @string)   => PrependToString(@string, value),
                (String @string, Value value)   => AppendToString(@string, value),

                _ => throw new Throw($"Cannot apply operator '+' on operands of types {a.GetType().ToString().ToLower()} and {b.GetType().ToString().ToLower()}"),
            };
        }

        private static Number AddScalars(IScalar left, IScalar right)
        {
            return new Number(left.GetDouble() + right.GetDouble());
        }

        private static String ConcatStrings(String left, String right)
        {
            return new String(left.Value + right.Value);
        }

        private static String PrependToString(String @string, Value value)
        {
            return new String(String.ImplicitCast(value).Value + @string.Value);
        }

        private static String AppendToString(String @string, Value value)
        {
            return new String(@string.Value + String.ImplicitCast(value).Value);
        }

        private static Array ConcatArrays(Array left, Array right)
        {
            var list = new List<Value>(left.Variables.Count + right.Variables.Count);

            foreach (var variable in left.Variables)
                list.Add(variable.Value.Copy());

            foreach (var variable in right.Variables)
                list.Add(variable.Value.Copy());

            return new Array(list);
        }

        private static Array PrependToArray(Array array, Value value)
        {
            var list = new List<Value>(array.Variables.Count + 1)
            {
                value
            };

            foreach (var variable in array.Variables)
                list.Add(variable.Value.Copy());

            return new Array(list);
        }

        private static Array AppendToArray(Array array, Value value)
        {
            var list = new List<Value>(array.Variables.Count + 1);

            foreach (var variable in array.Variables)
                list.Add(variable.Value.Copy());

            list.Add(value);

            return new Array(list);
        }

        private static Struct MergeStructs(Struct left, Struct right)
        {
            var dict = new Dictionary<string, Value>();

            foreach (var (key, variable) in left.Variables)
                dict[key] = variable.Value.Copy();

            foreach (var (key, variable) in right.Variables)
                dict[key] = variable.Value.Copy();

            return new Struct(dict);
        }
    }
}