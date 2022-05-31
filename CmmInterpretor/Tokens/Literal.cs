using CmmInterpretor.Expressions;

namespace CmmInterpretor.Tokens
{
    internal class Literal : Token
    {
        internal IExpression Expression { get; }

        internal override TokenType Type => TokenType.Literal;
        internal override string Text => "";

        internal Literal(int start, int end, IExpression expression) : base(start, end) => Expression = expression;
    }
}
