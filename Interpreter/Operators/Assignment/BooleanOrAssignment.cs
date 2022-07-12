using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Pointers;
using Bloc.Results;
using Bloc.Values;

namespace Bloc.Operators
{
    internal class BooleanOrAssignment : IExpression
    {
        private readonly IExpression _left;
        private readonly IExpression _right;

        internal BooleanOrAssignment(IExpression left, IExpression right)
        {
            _left = left;
            _right = right;
        }

        public IPointer Evaluate(Call call)
        {
            var value = _left.Evaluate(call);

            if (value is not Pointer pointer)
                throw new Throw("You cannot assign a value to a literal");

            if (!pointer.Get().Is(out Bool? @bool))
                throw new Throw("Cannot implicitly convert to bool");

            if (@bool!.Value)
                return value.Value;

            value = _right.Evaluate(call);

            return pointer.Set(value.Value);
        }
    }
}