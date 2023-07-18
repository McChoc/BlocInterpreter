using Bloc.Identifiers;
using Bloc.Memory;
using Bloc.Values;

namespace Bloc.Expressions.Operators;

internal sealed record NotDefOperator : IExpression
{
    private readonly NameIdentifier _identifier;

    internal NotDefOperator(NameIdentifier identifier)
    {
        _identifier = identifier;
    }

    public IValue Evaluate(Call call)
    {
        var pointer = call.Get(_identifier.Name);
        var isNotDefined = !pointer.IsDefined();

        return new Bool(isNotDefined);
    }
}