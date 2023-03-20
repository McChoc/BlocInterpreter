using Bloc.Memory;
using Bloc.Values;

namespace Bloc.Expressions.Literals;

internal sealed record NullLiteral : IExpression
{
    public IValue Evaluate(Call _) => Null.Value;
}