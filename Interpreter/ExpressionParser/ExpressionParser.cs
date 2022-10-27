using System.Collections.Generic;
using System.Linq;
using Bloc.Exceptions;
using Bloc.Expressions;
using Bloc.Extensions;
using Bloc.Scanners;
using Bloc.Tokens;

namespace Bloc
{
    internal delegate IExpression ParseDelegate(List<Token> tokens, int precedence);

    internal static partial class ExpressionParser
    {
        private static readonly ParseDelegate[] operations =
        {
            ParsePrimaries,
            ParseUnaries,
            ParseRanges,
            ParseCatchExpressions,
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
            ParseCoalescings,
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

        private static IExpression ParseBlock(Token token)
        {
            var tokens = TokenScanner.Scan(token).ToList();
            var parts = tokens.Split(x => x is (TokenType.Operator, ","));

            if (parts[^1].Count == 0)
                parts.RemoveAt(parts.Count - 1);

            if (parts.Count == 0)
                throw new SyntaxError(token.Start, token.End,
                    "Literal is ambiguous between an empty array and an empty struct. Use 'array()' or 'struct()' instead.");

            return parts[0].Any(x => x is (TokenType.Operator, "="))
                ? ParseStruct(parts)
                : ParseArray(parts);
        }

        private static ArrayLiteral ParseArray(List<List<Token>> parts)
        {
            var elements = new List<IExpression>();

            foreach (var part in parts)
            {
                if (part.Count == 0)
                    throw new SyntaxError(0, 0, "Unexpected symbol ','");

                if (part.Any(x => x is (TokenType.Operator, "=")))
                    throw new SyntaxError(0, 0, "Literal is ambiguous between an array and a struct.");

                var expression = Parse(part);

                elements.Add(expression);
            }

            return new ArrayLiteral(elements);
        }

        private static StructLiteral ParseStruct(List<List<Token>> parts)
        {
            var properties = new Dictionary<string, IExpression>();

            foreach (var part in parts)
            {
                if (part.Count == 0)
                    throw new SyntaxError(0, 0, "Unexpected symbol ','");

                var index = part.FindIndex(x => x is (TokenType.Operator, "="));

                if (index == -1)
                    throw new SyntaxError(0, 0, "Literal is ambiguous between an array and a struct.");

                var keyTokens = part.GetRange(..index);
                var valueTokens = part.GetRange((index + 1)..);

                if (keyTokens.Count == 0)
                    throw new SyntaxError(0, 0, "Missing identifier");

                if (valueTokens.Count == 0)
                    throw new SyntaxError(0, 0, "Missing value");

                var keyExpr = Parse(keyTokens);

                if (keyExpr is not Identifier identifier)
                    throw new SyntaxError(0, 0, "Invalid identifier");

                var expression = Parse(valueTokens);

                properties.Add(identifier.Name, expression);
            }

            return new StructLiteral(properties);
        }
    }
}