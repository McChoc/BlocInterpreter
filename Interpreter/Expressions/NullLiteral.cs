using Bloc.Memory;
using Bloc.Values;

namespace Bloc.Expressions;

internal sealed record NullLiteral : IExpression
{
    public IValue Evaluate(Call _) => Null.Value;
}