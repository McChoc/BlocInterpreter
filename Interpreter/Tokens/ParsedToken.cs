using Bloc.Expressions;
using Bloc.Utils.Attributes;

namespace Bloc.Tokens;

[Record]
internal sealed partial class ParsedToken : Token
{
    public IExpression Expression { get; set; }

    internal ParsedToken(int start, int end, IExpression expression)
        : base(start, end)
    {
        Expression = expression;
    }
}