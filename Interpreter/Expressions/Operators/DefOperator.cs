using Bloc.Identifiers;
using Bloc.Memory;
using Bloc.Values;

namespace Bloc.Expressions.Operators;

internal sealed record DefOperator : IExpression
{
    private readonly NameIdentifier _identifier;

    internal DefOperator(NameIdentifier identifier)
    {
        _identifier = identifier;
    }

    public IValue Evaluate(Call call)
    {
        var pointer = call.Get(_identifier.Name);
        var isDefined = pointer.IsDefined();

        return new Bool(isDefined);
    }
}