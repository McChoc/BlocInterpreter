using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Operators.Arithmetic;
using Bloc.Utils;
using Bloc.Values;

namespace Bloc.Operators.Assignment
{
    internal class RootAssignment : IExpression
    {
        private readonly IExpression _left;
        private readonly IExpression _right;

        internal RootAssignment(IExpression left, IExpression right)
        {
            _left = left;
            _right = right;
        }

        public IValue Evaluate(Call call)
        {
            var left = _left.Evaluate(call);
            var right = _right.Evaluate(call);

            return TupleUtil.RecursivelyCompoundAssign(left, right, Root.Operation);
        }
    }
}