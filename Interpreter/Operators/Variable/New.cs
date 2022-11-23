using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Pointers;
using Bloc.Values;
using Bloc.Variables;

namespace Bloc.Operators
{
    internal sealed record New : IExpression
    {
        private readonly IExpression _operand;

        internal New(IExpression operand)
        {
            _operand = operand;
        }

        public IPointer Evaluate(Call call)
        {
            var value = _operand.Evaluate(call).Value.Copy();
            value.Assign();

            var variable = new HeapVariable(value);
            var pointer = new VariablePointer(variable);

            return new Reference(pointer);
        }
    }
}