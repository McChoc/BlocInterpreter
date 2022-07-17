using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Pointers;
using Bloc.Results;
using Bloc.Values;
using Bloc.Variables;

namespace Bloc.Operators
{
    internal class Nameof : IExpression
    {
        private readonly IExpression _operand;

        internal Nameof(IExpression operand)
        {
            _operand = operand;
        }

        public IPointer Evaluate(Call call)
        {
            var value = _operand.Evaluate(call);

            if (value is VariablePointer pointer)
            {
                if (pointer.Variable is StackVariable variable)
                    return new String(variable.Name);

                if (pointer.Variable is StructVariable member)
                    return new String(member.Name);
            }

            throw new Throw("The expression must be a variable stored on the stack or a member of a struct");
        }
    }
}