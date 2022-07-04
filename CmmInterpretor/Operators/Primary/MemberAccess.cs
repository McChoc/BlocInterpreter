using CmmInterpretor.Expressions;
using CmmInterpretor.Memory;
using CmmInterpretor.Results;
using CmmInterpretor.Values;

namespace CmmInterpretor.Operators.Primary
{
    internal class MemberAccess : IExpression
    {
        private readonly IExpression _expression;
        private readonly string _member;

        internal MemberAccess(IExpression expression, string member)
        {
            _expression = expression;
            _member = member;
        }

        public IValue Evaluate(Call call)
        {
            var value = _expression.Evaluate(call);

            if (!value.Is(out Struct? obj))
                throw new Throw("The '.' operator can only be apllied to a struct");

            return obj!.Get(_member);
        }
    }
}