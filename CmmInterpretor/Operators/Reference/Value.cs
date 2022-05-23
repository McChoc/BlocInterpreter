using CmmInterpretor.Memory;
using CmmInterpretor.Expressions;
using CmmInterpretor.Results;
using CmmInterpretor.Values;

namespace CmmInterpretor.Operators.Reference
{
    public class Value : IExpression
    {
        private readonly IExpression _operand;

        public Value(IExpression operand) => _operand = operand;

        public IValue Evaluate(Call call)
        {
            var value = _operand.Evaluate(call);

            for (int i = 1; value.Is(out Values.Reference? reference); i++)
            {
                if (i >= call.Engine.HopLimit)
                    throw new Throw("The hop limit was reached");

                if (reference!.Variable is null)
                    throw new Throw("Invalid reference.");

                value = reference.Variable;
            }

            return value;
        }
    }
}
