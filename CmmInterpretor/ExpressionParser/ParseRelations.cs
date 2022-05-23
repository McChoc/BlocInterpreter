using CmmInterpretor.Utils.Exceptions;
using CmmInterpretor.Expressions;
using CmmInterpretor.Extensions;
using CmmInterpretor.Operators.Collection;
using CmmInterpretor.Operators.Relation;
using CmmInterpretor.Operators.Type;
using CmmInterpretor.Tokens;
using System.Collections.Generic;

namespace CmmInterpretor
{
    public static partial class ExpressionParser
    {
        private static IExpression ParseRelations(List<Token> tokens, int precedence)
        {
            for (int i = tokens.Count - 1; i >= 0; i--)
            {
                if (tokens[i] is { type: TokenType.Operator or TokenType.Keyword, value: "<" or "<=" or ">" or ">=" or "in" or "not in" or "is" or "is not" or "as" })
                {
                    if (i == 0)
                        throw new SyntaxError("Missing the left part of relation");

                    if (i > tokens.Count - 1)
                        throw new SyntaxError("Missing the right part of relation");

                    var a = ParseRelations(tokens.GetRange(..i), precedence);
                    var b = Parse(tokens.GetRange((i + 1)..), precedence - 1);

                    return tokens[i].value switch
                    {
                        "<" => new Less(a, b),
                        "<=" => new LessEqual(a, b),
                        ">" => new Greater(a, b),
                        ">=" => new GreaterEqual(a, b),
                        "in" => new In(a, b),
                        "not in" => new NotIn(a, b),
                        "is" => new Is(a, b),
                        "is not" => new IsNot(a, b),
                        "as" => new As(a, b),
                        _ => throw new System.Exception()
                    };
                }
            }

            return Parse(tokens, precedence - 1);
        }
    }
}
