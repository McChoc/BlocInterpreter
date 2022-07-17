using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Pointers;
using Bloc.Results;
using Bloc.Utils;
using Bloc.Values;

namespace Bloc.Operators
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

        public IPointer Evaluate(Call call)
        {
            var value = _expression.Evaluate(call).Value;

            value = ReferenceUtil.Dereference(value, call.Engine).Value;

            if (!value.Is(out Struct? @struct))
                throw new Throw("The '.' operator can only be apllied to a struct");

            return @struct!.Get(_member);
        }
    }
}