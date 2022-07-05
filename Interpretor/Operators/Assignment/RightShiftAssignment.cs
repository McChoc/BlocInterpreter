using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Operators.Bitwise;
using Bloc.Utils;
using Bloc.Values;

namespace Bloc.Operators.Assignment
{
    internal class RightShiftAssignment : IExpression
    {
        private readonly IExpression _left;
        private readonly IExpression _right;

        internal RightShiftAssignment(IExpression left, IExpression right)
        {
            _left = left;
            _right = right;
        }

        public IValue Evaluate(Call call)
        {
            var left = _left.Evaluate(call);
            var right = _right.Evaluate(call);

            return TupleUtil.RecursivelyCompoundAssign(left, right, RightShift.Operation);
        }
    }
}