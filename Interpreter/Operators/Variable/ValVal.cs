using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Pointers;
using Bloc.Utils;

namespace Bloc.Operators
{
    internal class ValVal : IExpression
    {
        private readonly IExpression _operand;

        internal ValVal(IExpression operand)
        {
            _operand = operand;
        }

        public IPointer Evaluate(Call call)
        {
            var value = _operand.Evaluate(call);

            return ReferenceUtil.TrueValue(value.Value, call.Engine);
        }
    }
}