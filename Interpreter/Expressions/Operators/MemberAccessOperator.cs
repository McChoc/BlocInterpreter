using Bloc.Memory;
using Bloc.Results;
using Bloc.Utils.Helpers;
using Bloc.Values;

namespace Bloc.Expressions.Operators;

internal sealed record MemberAccessOperator : IExpression
{
    private readonly IExpression _expression;
    private readonly string _member;

    internal MemberAccessOperator(IExpression expression, string member)
    {
        _expression = expression;
        _member = member;
    }

    public IValue Evaluate(Call call)
    {
        var value = _expression.Evaluate(call).Value;

        value = ReferenceHelper.Resolve(value, call.Engine.HopLimit).Value;

        if (value is not Struct @struct)
            throw new Throw("The '.' operator can only be apllied to a struct");

        return @struct.Get(_member);
    }
}