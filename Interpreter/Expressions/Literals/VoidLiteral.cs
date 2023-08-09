using Bloc.Memory;
using Bloc.Values.Core;
using Bloc.Values.Types;

namespace Bloc.Expressions.Literals;

internal sealed record VoidLiteral : IExpression
{
    public IValue Evaluate(Call _) => Void.Value;
}