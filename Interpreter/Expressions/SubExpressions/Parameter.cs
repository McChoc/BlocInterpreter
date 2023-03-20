using Bloc.Expressions;

namespace Bloc.Expressions.SubExpressions;

internal sealed record Parameter
{
    internal string Name { get; }
    internal IExpression? Expression { get; }

    internal Parameter(string name, IExpression? expression)
    {
        Name = name;
        Expression = expression;
    }
}