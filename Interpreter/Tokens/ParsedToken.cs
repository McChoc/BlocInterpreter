using Bloc.Expressions;

namespace Bloc.Tokens;

internal sealed class ParsedToken : Token
{
    public IExpression Expression { get; set; }

    internal ParsedToken(int start, int end, IExpression expression)
        : base(start, end)
    {
        Expression = expression;
    }
}