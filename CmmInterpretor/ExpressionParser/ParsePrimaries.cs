using System.Collections.Generic;
using System.Linq;
using CmmInterpretor.Expressions;
using CmmInterpretor.Extensions;
using CmmInterpretor.Operators.Primary;
using CmmInterpretor.Scanners;
using CmmInterpretor.Tokens;
using CmmInterpretor.Utils.Exceptions;
using CmmInterpretor.Values;

namespace CmmInterpretor
{
    internal static partial class ExpressionParser
    {
        private static IExpression ParsePrimaries(List<Token> tokens, int precedence)
        {
            var expression = tokens[0] switch
            {
                Literal literal => literal.Expression,

                (TokenType.Keyword, "void") => new VoidLiteral(),
                (TokenType.Keyword, "null") => new NullLiteral(),
                (TokenType.Keyword, "false") => new BoolLiteral(false),
                (TokenType.Keyword, "true") => new BoolLiteral(true),
                (TokenType.Keyword, "nan") => new NumberLiteral(double.NaN),
                (TokenType.Keyword, "infinity") => new NumberLiteral(double.PositiveInfinity),

                (TokenType.Keyword, "bool") => new TypeLiteral(ValueType.Bool),
                (TokenType.Keyword, "number") => new TypeLiteral(ValueType.Number),
                (TokenType.Keyword, "range") => new TypeLiteral(ValueType.Range),
                (TokenType.Keyword, "string") => new TypeLiteral(ValueType.String),
                (TokenType.Keyword, "tuple") => new TypeLiteral(ValueType.Tuple),
                (TokenType.Keyword, "array") => new TypeLiteral(ValueType.Array),
                (TokenType.Keyword, "struct") => new TypeLiteral(ValueType.Struct),
                (TokenType.Keyword, "function") => new TypeLiteral(ValueType.Function),
                (TokenType.Keyword, "task") => new TypeLiteral(ValueType.Task),
                (TokenType.Keyword, "reference") => new TypeLiteral(ValueType.Reference),
                (TokenType.Keyword, "complex") => new TypeLiteral(ValueType.Complex),
                (TokenType.Keyword, "type") => new TypeLiteral(ValueType.Type),

                (TokenType.Keyword, "recall") => new Recall(),
                (TokenType.Keyword, "params") => new Params(),

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
                        foreach (var part in content.Split(x => x is (TokenType.Operator, ",")))
                            parameters.Add(Parse(part));

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