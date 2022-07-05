using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Values;

namespace Bloc.Operators.Primary
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