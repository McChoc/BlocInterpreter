using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Pointers;
using Bloc.Values;

namespace Bloc.Operators
{
    internal class Inequality : IExpression
    {
        private readonly IExpression _left;
        private readonly IExpression _right;

        internal Inequality(IExpression left, IExpression right)
        {
            _left = left;
            _right = right;
        }

        public IPointer Evaluate(Call call)
        {
            var left = _left.Evaluate(call).Value;
            var right = _right.Evaluate(call).Value;

            return new Bool(!left.Equals(right!));
        }
    }
}