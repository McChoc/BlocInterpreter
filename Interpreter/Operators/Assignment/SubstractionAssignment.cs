using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Pointers;
using Bloc.Utils;

namespace Bloc.Operators
{
    internal class SubstractionAssignment : IExpression
    {
        private readonly IExpression _left;
        private readonly IExpression _right;

        internal SubstractionAssignment(IExpression left, IExpression right)
        {
            _left = left;
            _right = right;
        }

        public IPointer Evaluate(Call call)
        {
            var left = _left.Evaluate(call);
            var right = _right.Evaluate(call);

            return AssignmentUtil.CompoundAssign(left, right, Substraction.Operation, call);
        }
    }
}