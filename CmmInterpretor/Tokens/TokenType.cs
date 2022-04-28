namespace CmmInterpretor.Tokens
{
    public enum TokenType
    {
        Empty,
        Operator,
        Keyword,
        Identifier,
        Literal,
        Interpolated,
        Parentheses,
        Brackets,
        Block,
        CodeBlock,
        Command
    }
}
