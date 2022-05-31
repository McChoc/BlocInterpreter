using CmmInterpretor.Utils.Exceptions;
using CmmInterpretor.Expressions;
using CmmInterpretor.Extensions;
using CmmInterpretor.Operators.Assignment;
using CmmInterpretor.Tokens;
using System.Collections.Generic;

namespace CmmInterpretor
{
    internal static partial class ExpressionParser
    {
        private static IExpression ParseAssignments(List<Token> tokens, int precedence)
        {
            for (int i = 0; i < tokens.Count; i++)
            {
                if (tokens[i] is (TokenType.Operator, "=" or "+=" or "-=" or "*=" or "/=" or "%=" or "**=" or "//=" or "%%=" or "&&=" or "||=" or "^^=" or "&=" or "|=" or "^=" or "<<=" or ">>=") op)
                {
                    if (i == 0)
                        throw new SyntaxError(op.Start, op.End, "Missing the left part of assignment");

                    if (i > tokens.Count - 1)
                        throw new SyntaxError(op.Start, op.End, "Missing the right part of assignment");

                    var a = Parse(tokens.GetRange(..i), precedence - 1);
                    var b = ParseAssignments(tokens.GetRange((i + 1)..), precedence);

                    return op.Text switch
                    {
                        "=" => new Assignment(a, b),
                        "+=" => new AdditionAssignment(a, b),
                        "-=" => new SubstractionAssignment(a, b),
                        "*=" => new MultiplicationAssignment(a, b),
                        "/=" => new DivisionAssignment(a, b),
                        "%=" => new RemainderAssignment(a, b),
                        "**=" => new PowerAssignment(a, b),
                        "//=" => new RootAssignment(a, b),
                        "%%=" => new LogarithmAssignment(a, b),
                        "&&=" => new BooleanAndAssignment(a, b),
                        "||=" => new BooleanOrAssignment(a, b),
                        "^^=" => new BooleanXorAssignment(a, b),
                        "&=" => new BitwiseAndAssignment(a, b),
                        "|=" => new BitwiseOrAssignment(a, b),
                        "^=" => new BitwiseXorAssignment(a, b),
                        "<<=" => new LeftShiftAssignment(a, b),
                        ">>=" => new RightShiftAssignment(a, b),
                        _ => throw new System.Exception()
                    };
                }
            }

            return Parse(tokens, precedence - 1);
        }
    }
}
