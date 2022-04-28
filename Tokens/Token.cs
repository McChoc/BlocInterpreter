namespace CmmInterpretor.Tokens
{
    public struct Token
    {
        public static Token Comma => new(TokenType.Operator, ",");

        public string Text => (string)value;

        public TokenType type;
        public object value;

        public Token(TokenType type, object value)
        {
            this.type = type;
            this.value = value;
        }
    }
}
