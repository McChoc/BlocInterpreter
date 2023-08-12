using Bloc.Identifiers;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Utils.Helpers;
using Bloc.Values.Core;
using Bloc.Values.Types;

namespace Bloc.Expressions.Operators;

internal sealed record MemberAccessOperator : IExpression
{
    private readonly IExpression _expression;
    private readonly INamedIdentifier _identifier;

    internal MemberAccessOperator(IExpression expression, INamedIdentifier identifier)
    {
        _expression = expression;
        _identifier = identifier;
    }

    public IValue Evaluate(Call call)
    {
        var value = _expression.Evaluate(call).Value;

        value = ReferenceHelper.Resolve(value, call.Engine.Options.HopLimit).Value;

        if (value is not Struct @struct)
            throw new Throw("The '.' operator can only be apllied to structs");

        var name = _identifier.GetName(call);

        return @struct.GetMember(name);
    }
}