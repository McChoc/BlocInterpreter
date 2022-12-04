using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Utils;
using Bloc.Values;

namespace Bloc.Operators
{
    internal sealed record RemainderAssignment : IExpression
    {
        private readonly IExpression _left;
        private readonly IExpression _right;

        internal RemainderAssignment(IExpression left, IExpression right)
        {
            _left = left;
            _right = right;
        }

        public IValue Evaluate(Call call)
        {
            var left = _left.Evaluate(call);
            var right = _right.Evaluate(call).Value.Copy();

            return AssignmentUtil.CompoundAssign(left, right, Remainder.Operation, call);
        }
    }
}