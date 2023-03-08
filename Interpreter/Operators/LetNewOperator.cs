using Bloc.Expressions;
using Bloc.Identifiers;
using Bloc.Memory;
using Bloc.Values;

namespace Bloc.Operators;

internal sealed record LetNewOperator : IExpression
{
    private readonly IIdentifier _identifier;

    internal LetNewOperator(IIdentifier identifier)
    {
        _identifier = identifier;
    }

    public IValue Evaluate(Call call)
    {
        return _identifier.Define(Null.Value, call, true);
    }
}