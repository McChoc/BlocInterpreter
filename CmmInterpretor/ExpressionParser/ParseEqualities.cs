using System.Collections.Generic;
using CmmInterpretor.Expressions;
using CmmInterpretor.Extensions;
using CmmInterpretor.Operators.Relation;
using CmmInterpretor.Tokens;
using CmmInterpretor.Utils.Exceptions;

namespace CmmInterpretor
{
    internal static partial class ExpressionParser
    {
        private static IExpression ParseEqualities(List<Token> tokens, int precedence)
        {
            for (var i = tokens.Count - 1; i >= 0; i--)
            {
                if (tokens[i] is (TokenType.Operator, "==" or "!=" or "<>") op)
                {
                    if (i == 0)
                        throw new SyntaxError(op.Start, op.End, "Missing the left part of equality");

                    if (i > tokens.Count - 1)
                        throw new SyntaxError(op.Start, op.End, "Missing the right part of equality");

                    var a = ParseEqualities(tokens.GetRange(..i), precedence);
                    var b = Parse(tokens.GetRange((i + 1)..), precedence - 1);

                    return op.Text switch
                    {
                        "==" => new Equality(a, b),
                        _ => new Inequality(a, b)
                    };
                }
            }

            return Parse(tokens, precedence - 1);
        }
    }
}