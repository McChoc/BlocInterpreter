using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bloc.Expressions;
using Bloc.Interfaces;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Utils;
using Bloc.Values;

namespace Bloc.Operators
{
    internal sealed record Multiplication : IExpression
    {
        private readonly IExpression _left;
        private readonly IExpression _right;

        internal Multiplication(IExpression left, IExpression right)
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
                (IScalar left, IScalar right)       => MultiplyScalars(left, right),
                (String @string, IScalar scalar)    => MultiplyString(@string, scalar),
                (IScalar scalar, String @string)    => MultiplyString(@string, scalar),
                (Array array, IScalar scalar)       => Multiply(array, scalar),
                (IScalar scalar, Array array)       => Multiply(array, scalar),

                _ => throw new Throw($"Cannot apply operator '*' on operands of types {a.GetType().ToString().ToLower()} and {b.GetType().ToString().ToLower()}"),
            };
        }

        private static Number MultiplyScalars(IScalar left, IScalar right)
        {
            return new Number(left.GetDouble() * right.GetDouble());
        }

        private static String MultiplyString(String @string, IScalar scalar)
        {
            int count = scalar.GetInt();

            if (count < 0)
                throw new Throw("You cannot multiply a string by a negative number");

            var builder = new StringBuilder(@string.Value.Length * count);

            for (var i = 0; i < count; i++)
                builder.Append(@string.Value);

            return new String(builder.ToString());
        }

        private static Array Multiply(Array array, IScalar scalar)
        {
            int count = scalar.GetInt();

            if (count < 0)
                throw new Throw("You cannot multiply an array by a negative number");

            var list = new List<Value>(array.Variables.Count * count);

            for (var i = 0; i < count; i++)
                list.AddRange(array.Variables.Select(x => x.Value.Copy()));

            return new Array(list);
        }
    }
}