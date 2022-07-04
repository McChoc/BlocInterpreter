using System;
using System.Collections.Generic;
using CmmInterpretor.Expressions;
using CmmInterpretor.Extensions;
using CmmInterpretor.Operators.Collection;
using CmmInterpretor.Operators.Relation;
using CmmInterpretor.Operators.Type;
using CmmInterpretor.Tokens;
using CmmInterpretor.Utils.Exceptions;

namespace CmmInterpretor
{
    internal static partial class ExpressionParser
    {
        private static IExpression ParseRelations(List<Token> tokens, int precedence)
        {
            for (var i = tokens.Count - 1; i >= 0; i--)
            {
                if (tokens[i] is (TokenType.Operator or TokenType.Keyword,
                    "<" or "<=" or ">" or ">=" or "in" or "not in" or "is" or "is not" or "as") op)
                {
                    if (i == 0)
                        throw new SyntaxError(op.Start, op.End, "Missing the left part of relation");

                    if (i > tokens.Count - 1)
                        throw new SyntaxError(op.Start, op.End, "Missing the right part of relation");

                    var a = ParseRelations(tokens.GetRange(..i), precedence);
                    var b = Parse(tokens.GetRange((i + 1)..), precedence - 1);

                    return op.Text switch
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
                        _ => throw new Exception()
                    };
                }
            }

            return Parse(tokens, precedence - 1);
        }
    }
}