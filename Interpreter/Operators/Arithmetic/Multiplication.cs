using System.Collections.Generic;
using System.Text;
using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Utils;
using Bloc.Values;

namespace Bloc.Operators.Arithmetic
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

        public IValue Evaluate(Call call)
        {
            var left = _left.Evaluate(call);
            var right = _right.Evaluate(call);

            return TupleUtil.RecursivelyCall(left, right, Operation);
        }

        internal static IValue Operation(IValue left, IValue right)
        {
            if (left.Value.Is(out Number? leftNumber) && right.Value.Is(out Number? rightNumber))
                return new Number(leftNumber!.Value * rightNumber!.Value);

            if ((left.Value.Is(out Array? array) && right.Value.Is(out Number? number)) ||
                (left.Value.Is(out number) && right.Value.Is(out array)))
            {
                if (number!.Value < 0)
                    throw new Throw("You cannot multiply an array by a negative number");

                var list = new List<IValue>();

                var amount = number!.ToInt();

                for (var i = 0; i < amount; i++)
                    list.AddRange(((Array)array!.Copy()).Values);

                return new Array(list);
            }

            if ((left.Value.Is(out String? str) && right.Value.Is(out number)) || (left.Value.Is(out number) && right.Value.Is(out str)))
            {
                if (number!.Value < 0)
                    throw new Throw("You cannot multiply a string by a negative number");

                var builder = new StringBuilder();

                var amount = number!.ToInt();

                for (var i = 0; i < amount; i++)
                    builder.Append(str!.Value);

                return new String(builder.ToString());
            }

            throw new Throw($"Cannot apply operator '*' on operands of types {left.GetType().ToString().ToLower()} and {right.GetType().ToString().ToLower()}");
        }
    }
}