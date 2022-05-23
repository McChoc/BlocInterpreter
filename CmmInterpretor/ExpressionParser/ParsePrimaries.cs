using CmmInterpretor.Utils.Exceptions;
using CmmInterpretor.Expressions;
using CmmInterpretor.Extensions;
using CmmInterpretor.Operators.Primary;
using CmmInterpretor.Tokens;
using System.Collections.Generic;

namespace CmmInterpretor
{
    public static partial class ExpressionParser
    {
        private static IExpression ParsePrimaries(List<Token> tokens, int precedence)
        {
            IExpression expression = tokens[0] switch
            {
                { type: TokenType.Keyword, value: "recall" } => new Recall(),
                { type: TokenType.Keyword, value: "params" } => new Params(),
                { type: TokenType.Literal } => (IExpression)tokens[0].value,
                { type: TokenType.Interpolated } => ParseInterpolatedString(tokens[0]),
                { type: TokenType.Block } => ParseBlock(tokens[0]),
                { type: TokenType.Parentheses } => Parse((List<Token>)tokens[0].value),
                { type: TokenType.Identifier, value: string identifier } => new Identifier(identifier),
                _ => throw new SyntaxError("Unexpected symbol"),
            };

            for (int i = 1; i < tokens.Count; i++)
            {
                if (tokens[i] is { type : TokenType.Operator, Text : "." })
                {
                    if (tokens.Count <= i)
                        throw new SyntaxError("Missing identifier");

                    Token identifier = tokens[i + 1];

                    if (identifier.type != TokenType.Identifier)
                        throw new SyntaxError("Missing identifier");

                    expression = new MemberAccess(expression, identifier.Text);

                    i++;
                }
                else if (tokens[i].type == TokenType.Brackets)
                {
                    var index = Parse((List<Token>)tokens[i].value);

                    expression = new Indexer(expression, index);
                }
                else if (tokens[i].type == TokenType.Parentheses)
                {
                    var content = (List<Token>)tokens[i].value;

                    var parameters = new List<IExpression>();

                    if (content.Count > 0)
                        foreach (var part in content.Split(Token.Comma))
                            parameters.Add(Parse(part));

                    expression = new Invocation(expression, parameters);
                }
                else
                {
                    throw new SyntaxError("Unexpected symbol");
                }
            }

            return expression;
        }
    }
}
