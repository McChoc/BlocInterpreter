using CmmInterpretor.Memory;
using CmmInterpretor.Expressions;
using CmmInterpretor.Values;
using CmmInterpretor.Operators.Arithmetic;
using CmmInterpretor.Utils;

namespace CmmInterpretor.Operators.Assignment
{
    internal class AdditionAssignment : IExpression
    {
        private readonly IExpression _left;
        private readonly IExpression _right;

        internal AdditionAssignment(IExpression left, IExpression right)
        {
            _left = left;
            _right = right;
        }

        public IValue Evaluate(Call call)
        {
            var left = _left.Evaluate(call);
            var right = _right.Evaluate(call);

            return TupleUtil.RecursivelyCompoundAssign(left, right, Addition.Operation);
        }
    }
}
