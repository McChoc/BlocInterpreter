using CmmInterpretor.Memory;
using CmmInterpretor.Expressions;
using CmmInterpretor.Results;
using CmmInterpretor.Values;
using System.Collections.Generic;
using System.Text;
using CmmInterpretor.Utils;

namespace CmmInterpretor.Operators.Arithmetic
{
    public class Multiplication : IExpression
    {
        private readonly IExpression _left;
        private readonly IExpression _right;

        public Multiplication(IExpression left, IExpression right)
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
            if (left.Is(out Number? leftNumber) && right.Is(out Number? rightNumber))
                return new Number(leftNumber!.Value * rightNumber!.Value);

            if (left.Is(out Array? array) && right.Is(out Number? number) || left.Is(out number) && right.Is(out array))
            {
                if (number!.Value < 0)
                    throw new Throw("You cannot multiply an array by a negative number");

                var list = new List<IValue>();

                int amount = number!.ToInt();

                for (int i = 0; i < amount; i++)
                    list.AddRange(((Array)array!.Copy()).Values);

                return new Array(list);
            }

            if (left.Is(out String? str) && right.Is(out number) || left.Is(out number) && right.Is(out str))
            {
                if (number!.Value < 0)
                    throw new Throw("You cannot multiply a string by a negative number");

                var builder = new StringBuilder();

                int amount = number!.ToInt();

                for (int i = 0; i < amount; i++)
                    builder.Append(str!.Value);

                return new String(builder.ToString());
            }

            throw new Throw($"Cannot apply operator '*' on operands of types {left.Type.ToString().ToLower()} and {right.Type.ToString().ToLower()}");
        }
    }
}
