using CmmInterpretor.Memory;
using CmmInterpretor.Expressions;
using CmmInterpretor.Results;
using CmmInterpretor.Values;
using CmmInterpretor.Variables;

namespace CmmInterpretor.Operators.Info
{
    public class Nameof : IExpression
    {
        private readonly IExpression _operand;

        public Nameof(IExpression operand) => _operand = operand;

        public IValue Evaluate(Call call)
        {
            var value = _operand.Evaluate(call);

            if (value is not StackVariable variable)
                throw new Throw("The expression must be a variable stored on the stack");

            return new String(variable.Name);
        }
    }
}
