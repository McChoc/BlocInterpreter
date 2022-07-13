using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Pointers;
using Bloc.Results;
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

            for (var i = 1; value.Value.Is(out Reference? reference); i++)
            {
                if (i > call.Engine.HopLimit)
                    throw new Throw("The hop limit was reached");

                value = reference!.Pointer;
            }

            return value;
        }
    }
}