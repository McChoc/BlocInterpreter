using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Pointers;
using Bloc.Values;

namespace Bloc.Operators
{
    internal class Typeof : IExpression
    {
        private readonly IExpression _operand;

        internal Typeof(IExpression operand)
        {
            _operand = operand;
        }

        public IPointer Evaluate(Call call)
        {
            var value = _operand.Evaluate(call).Value;

            return new Type(value.GetType());
        }
    }
}