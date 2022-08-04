using System.Collections.Generic;
using System.Linq;
using Bloc.Exceptions;
using Bloc.Expressions;
using Bloc.Extensions;
using Bloc.Operators;
using Bloc.Scanners;
using Bloc.Tokens;

namespace Bloc
{
    internal static partial class ExpressionParser
    {
        private static IExpression ParsePrimaries(List<Token> tokens, int precedence)
        {
            var expression = tokens[0] switch
            {
                Literal literal => literal.Expression,

                { Type: TokenType.Identifier } => new Identifier(tokens[0].Text),
                { Type: TokenType.Braces } => ParseBlock(tokens[0]),
                { Type: TokenType.Parentheses } => Parse(TokenScanner.Scan(tokens[0]).ToList()),

                _ => throw new SyntaxError(tokens[0].Start, tokens[0].End, "Unexpected symbol")
            };

            for (var i = 1; i < tokens.Count; i++)
            {
                if (tokens[i] is (TokenType.Operator, "."))
                {
                    if (tokens.Count <= i)
                        throw new SyntaxError(tokens[i].Start, tokens[i].End, "Missing identifier");

                    if (tokens[i + 1].Type != TokenType.Identifier)
                        throw new SyntaxError(tokens[i].Start, tokens[i].End, "Missing identifier");

                    expression = new MemberAccess(expression, tokens[i + 1].Text);

                    i++;
                }
                else if (tokens[i].Type == TokenType.Brackets)
                {
                    var index = Parse(TokenScanner.Scan(tokens[i]).ToList());

                    expression = new Indexer(expression, index);
                }
                else if (tokens[i].Type == TokenType.Parentheses)
                {
                    var content = TokenScanner.Scan(tokens[i]).ToList();

                    var parameters = new List<IExpression>();

                    if (content.Count > 0)
                    {
                        foreach (var part in content.Split(x => x is (TokenType.Operator, ",")))
                        {
                            if (part.Count == 0)
                                parameters.Add(new VoidLiteral());
                            else
                                parameters.Add(Parse(part));
                        }
                    }

                    expression = new Invocation(expression, parameters);
                }
                else
                {
                    throw new SyntaxError(tokens[i].Start, tokens[i].End, "Unexpected symbol");
                }
            }

            return expression;
        }
    }
}