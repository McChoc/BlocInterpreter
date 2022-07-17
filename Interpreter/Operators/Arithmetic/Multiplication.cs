using System.Collections.Generic;
using System.Text;
using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Pointers;
using Bloc.Results;
using Bloc.Utils;
using Bloc.Values;
using Bloc.Variables;

namespace Bloc.Operators
{
    internal class Multiplication : IExpression
    {
        private readonly IExpression _left;
        private readonly IExpression _right;

        internal Multiplication(IExpression left, IExpression right)
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

        internal static Value Operation(Value left, Value right)
        {
            if (left.Is(out Number? leftNumber) && right.Is(out Number? rightNumber))
                return new Number(leftNumber!.Value * rightNumber!.Value);

            if ((left.Is(out Array? array) && right.Is(out Number? number)) ||
                (left.Is(out number) && right.Is(out array)))
            {
                if (number!.Value < 0)
                    throw new Throw("You cannot multiply an array by a negative number");

                var amount = number.ToInt();

                var list = new List<IVariable>(array!.Values.Count * amount);

                for (var i = 0; i < amount; i++)
                    list.AddRange(((Array)array.Copy()).Values);

                return new Array(list);
            }

            if ((left.Is(out String? str) && right.Is(out number)) ||
                (left.Is(out number) && right.Is(out str)))
            {
                if (number!.Value < 0)
                    throw new Throw("You cannot multiply a string by a negative number");

                var amount = number!.ToInt();

                var builder = new StringBuilder(str!.Value.Length * amount);

                for (var i = 0; i < amount; i++)
                    builder.Append(str.Value);

                return new String(builder.ToString());
            }

            throw new Throw($"Cannot apply operator '*' on operands of types {left.GetType().ToString().ToLower()} and {right.GetType().ToString().ToLower()}");
        }
    }
}