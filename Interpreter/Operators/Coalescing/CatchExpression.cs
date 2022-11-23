using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Pointers;
using Bloc.Results;

namespace Bloc.Operators
{
    internal sealed record CatchExpression : IExpression
    {
        private readonly IExpression _left;
        private readonly IExpression _right;

        internal CatchExpression(IExpression left, IExpression right)
        {
            _left = left;
            _right = right;
        }

        public IPointer Evaluate(Call call)
        {
            try
            {
                return _left.Evaluate(call).Value;
            }
            catch (Throw)
            {
                return _right.Evaluate(call).Value;
            }
        }
    }
}