using Bloc.Memory;
using Bloc.Values.Core;
using Bloc.Values.Types;

namespace Bloc.Expressions.Literals;

internal sealed record TypeLiteral : IExpression
{
    private readonly ValueType _type;

    internal TypeLiteral(ValueType type) => _type = type;

    public IValue Evaluate(Call _) => new Type(_type);
}