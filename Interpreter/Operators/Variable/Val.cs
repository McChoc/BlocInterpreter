using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Pointers;
using Bloc.Results;
using Bloc.Utils;
using Bloc.Values;

namespace Bloc.Operators
{
    internal class Val : IExpression
    {
        private readonly IExpression _operand;

        internal Val(IExpression operand)
        {
            _operand = operand;
        }

        public IPointer Evaluate(Call call)
        {
            var value = _operand.Evaluate(call);

            if (value is not Reference and not Pointer)
                throw new Throw("The 'val' operator can only be used on references and variables");

            return ReferenceUtil.Dereference(value, call.Engine.HopLimit);
        }
    }
}