using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Values;
using Bloc.Variables;

namespace Bloc.Operators.Misc
{
    internal class Nameof : IExpression
    {
        private readonly IExpression _operand;

        internal Nameof(IExpression operand)
        {
            _operand = operand;
        }

        public IValue Evaluate(Call call)
        {
            var value = _operand.Evaluate(call);

            if (value is not StackVariable variable)
                throw new Throw("The expression must be a variable stored on the stack");

            return new String(variable.Name);
        }
    }
}