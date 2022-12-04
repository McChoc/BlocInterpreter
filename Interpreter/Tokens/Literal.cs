using Bloc.Expressions;

namespace Bloc.Tokens
{
    internal sealed class Literal : Token
    {
        internal Literal(int start, int end, IExpression expression) : base(start, end)
        {
            Expression = expression;
        }

        internal IExpression Expression { get; }

        internal override TokenType Type => TokenType.Literal;
        internal override string Text => "";

        public override int GetHashCode()
        {
            return System.HashCode.Combine(Expression);
        }

        public override bool Equals(object other)
        {
            return other is Literal literal &&
                Expression == literal.Expression;
        }
    }
}