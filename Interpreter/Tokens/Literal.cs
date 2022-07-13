using Bloc.Expressions;

namespace Bloc.Tokens
{
    internal class Literal : Token
    {
        internal Literal(int start, int end, IExpression expression) : base(start, end)
        {
            Expression = expression;
        }

        internal IExpression Expression { get; }

        internal override TokenType Type => TokenType.Literal;
        internal override string Text => "";
    }
}