using Bloc.Expressions;
using Bloc.Identifiers;
using Bloc.Memory;
using Bloc.Values;

namespace Bloc.Expressions.Operators;

internal sealed record LetOperator : IExpression
{
    private readonly IIdentifier _identifier;

    internal LetOperator(IIdentifier identifier)
    {
        _identifier = identifier;
    }

    public IValue Evaluate(Call call)
    {
        return _identifier.Define(Null.Value, call);
    }
}