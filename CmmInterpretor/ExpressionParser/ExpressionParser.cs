using System.Collections.Generic;
using System.Linq;
using CmmInterpretor.Expressions;
using CmmInterpretor.Extensions;
using CmmInterpretor.Scanners;
using CmmInterpretor.Tokens;
using CmmInterpretor.Utils.Exceptions;

namespace CmmInterpretor
{
    internal delegate IExpression ParseDelegate(List<Token> tokens, int precedence);

    internal static partial class ExpressionParser
    {
        private static readonly ParseDelegate[] operations =
        {
            ParsePrimaries,
            ParseUnaries,
            ParseRanges,
            ParseExponentials,
            ParseMultiplicatives,
            ParseAdditives,
            ParseShifts,
            ParseComparisons,
            ParseRelations,
            ParseEqualities,
            ParseLogicalANDs,
            ParseLogicalXORs,
            ParseLogicalORs,
            ParseConditionalANDs,
            ParseConditionalXORs,
            ParseConditionalORs,
            ParseTernaries,
            ParseAssignments,
            ParseFunctions,
            ParseTuples
        };

        internal static IExpression Parse(List<Token> expression)
        {
            return Parse(expression, operations.Length - 1);
        }

        private static IExpression Parse(List<Token> expression, int precedence)
        {
            return operations[precedence](expression, precedence);
        }

        internal static IExpression ParseBlock(Token token)
        {
            var tokens = TokenScanner.Scan(token).ToList();

            if (tokens.Count == 0)
                throw new SyntaxError(token.Start, token.End,
                    "Literal is ambiguous between an empty array and an empty struct. Use 'array()' or 'struct()' instead.");

            var parts = tokens.Split(x => x is (TokenType.Operator, ","));

            if (parts[^1].Count == 0)
                parts.RemoveAt(parts.Count - 1);

            if (parts.All(p => p.Count >= 2 && p[0].Type == TokenType.Identifier && p[1] is (TokenType.Operator, "=")))
                return new StrucLiteral(parts.ToDictionary(p => p[0].Text, p => Parse(p.GetRange(2..))));
            return new ArrayLiteral(parts.Select(p => Parse(p)).ToList());
        }
    }
}