using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Pointers;
using Bloc.Utils;

namespace Bloc.Operators
{
    internal class LeftShiftAssignment : IExpression
    {
        private readonly IExpression _left;
        private readonly IExpression _right;

        internal LeftShiftAssignment(IExpression left, IExpression right)
        {
            _left = left;
            _right = right;
        }

        public IPointer Evaluate(Call call)
        {
            var left = _left.Evaluate(call);
            var right = _right.Evaluate(call);

            return TupleUtil.RecursivelyCompoundAssign(left, right, LeftShift.Operation);
        }
    }
}