using System.Collections.Generic;
using Bloc.Expressions;
using Bloc.Interfaces;
using Bloc.Memory;
using Bloc.Pointers;
using Bloc.Results;
using Bloc.Utils;
using Bloc.Values;
using Bloc.Variables;

namespace Bloc.Operators
{
    internal sealed record Substraction : IExpression
    {
        private readonly IExpression _left;
        private readonly IExpression _right;

        internal Substraction(IExpression left, IExpression right)
        {
            _left = left;
            _right = right;
        }

        public IPointer Evaluate(Call call)
        {
            var left = _left.Evaluate(call).Value;
            var right = _right.Evaluate(call).Value;

            return OperatorUtil.RecursivelyCall(left, right, Operation, call);
        }

        internal static Value Operation(Value a, Value b)
        {
            return (a, b) switch
            {
                (IScalar left, IScalar right)   => SubstractScalars(left, right),
                (Array array, Value value)      => RemoveFromArray(array, value),

                _ => throw new Throw($"Cannot apply operator '-' on operands of types {a.GetType().ToString().ToLower()} and {b.GetType().ToString().ToLower()}"),
            };
        }

        private static Number SubstractScalars(IScalar left, IScalar right)
        {
            return new Number(left.GetDouble() - right.GetDouble());
        }

        private static Array RemoveFromArray(Array array, Value value)
        {
            bool found = false;

            var list = new List<IVariable>(array.Values.Count - 1);

            foreach (var item in array.Values)
            {
                if (!found && item.Value.Equals(value))
                    found = true;
                else
                    list.Add(item.Value.Copy());
            }

            return new Array(list);
        }
    }
}