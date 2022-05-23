using CmmInterpretor.Utils.Exceptions;
using CmmInterpretor.Expressions;
using CmmInterpretor.Extensions;
using CmmInterpretor.Tokens;
using System.Collections.Generic;
using System.Linq;
using CmmInterpretor.Scanners;

namespace CmmInterpretor
{
    internal delegate IExpression ParseDelegate(List<Token> tokens, int precedence);

    public static partial class ExpressionParser
    {
        private static readonly ParseDelegate[] operations = new ParseDelegate[]
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

        public static IExpression Parse(List<Token> expression) => Parse(expression, operations.Length - 1);

        private static IExpression Parse(List<Token> expression, int precedence) => operations[precedence](expression, precedence);

        public static IExpression ParseInterpolatedString(Token token)
        {
            var (baseString, expressions) = ((string, List<(int, List<Token>)>))token.value;

            return new InterpolatedString(baseString, expressions.Select(e => (e.Item1, Parse(e.Item2))).ToList());
        }

        public static IExpression ParseBlock(Token token)
        {
            var scanner = new TokenScanner((string)token.value);

            var tokens = new List<Token>();

            while (scanner.HasNextToken())
                tokens.Add(scanner.GetNextToken());

            if (tokens.Count == 0)
                throw new SyntaxError("Literal is ambiguous between an empty array and an empty struct. Use 'array()' or 'struct()' instead.");

            var parts = tokens.Split(Token.Comma);

            if (parts[^1].Count == 0)
                parts.RemoveAt(parts.Count - 1);

            if (parts.All(p => p.Count >= 2 && p[0].type == TokenType.Identifier && p[1] is { type: TokenType.Operator, value: "=" }))
                return new StrucLiteral(parts.ToDictionary(p => p[0].Text, p => Parse(p.GetRange(2..))));
            else
                return new ArrayLiteral(parts.Select(p => Parse(p)).ToList());
        }
    }
}
