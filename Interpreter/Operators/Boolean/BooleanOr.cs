using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Pointers;
using Bloc.Results;
using Bloc.Values;

namespace Bloc.Operators
{
    internal class BooleanOr : IExpression
    {
        private readonly IExpression _left;
        private readonly IExpression _right;

        internal BooleanOr(IExpression left, IExpression right)
        {
            _left = left;
            _right = right;
        }

        public IPointer Evaluate(Call call)
        {
            var value = _left.Evaluate(call).Value;

            if (!value.Is(out Bool? @bool))
                throw new Throw("Cannot implicitly convert to bool");

            if (@bool!.Value)
                return value;

            return _right.Evaluate(call).Value;
        }
    }
}