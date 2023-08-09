using Bloc.Memory;
using Bloc.Values.Core;
using Bloc.Values.Types;

namespace Bloc.Expressions.Literals;

internal sealed record NumberLiteral : IExpression
{
    private readonly double _number;

    internal NumberLiteral(double number) => _number = number;

    public IValue Evaluate(Call _) => new Number(_number);
}