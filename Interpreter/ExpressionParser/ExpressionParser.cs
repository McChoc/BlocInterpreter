using System.Collections.Generic;
using System.Linq;
using Bloc.Exceptions;
using Bloc.Expressions;
using Bloc.Extensions;
using Bloc.Scanners;
using Bloc.Tokens;

namespace Bloc
{
    internal static partial class ExpressionParser
    {
        private delegate IExpression ParseCallback(List<Token> tokens, int precedence);

        private static readonly ParseCallback[] operations =
        {
            ParsePrimaries,
            ParseUnaries,
            ParseRanges,
            ParseQueries,
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
            ParseCatchExpressions,
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

            if (parts[0][0] is (TokenType.Operator, ".."))
                return ParseArray(parts);

            if (parts[0][0] is (TokenType.Operator, "..."))
                return ParseStruct(parts);

            return parts[0].Any(x => x is (TokenType.Operator, "="))
                ? ParseStruct(parts)
                : ParseArray(parts);
        }

        private static ArrayLiteral ParseArray(List<List<Token>> parts)
        {
            var expressions = new List<ArrayLiteral.SubExpression>();

            foreach (var part in parts)
            {
                if (part.Count == 0)
                    throw new SyntaxError(0, 0, "Unexpected symbol ','");

                if (part.Any(x => x is (TokenType.Operator, "=")))
                    throw new SyntaxError(0, 0, "Literal is ambiguous between an array and a struct.");

                if (part[0] is (TokenType.Operator, "..."))
                    throw new SyntaxError(0, 0, "Literal is ambiguous between an array and a struct.");

                if (part[0] is (TokenType.Operator, ".."))
                    expressions.Add(new(true, Parse(part.GetRange(1..))));
                else
                    expressions.Add(new(false, Parse(part)));
            }

            return new ArrayLiteral(expressions);
        }

        private static StructLiteral ParseStruct(List<List<Token>> parts)
        {
            var expressions = new List<StructLiteral.SubExpression>();

            foreach (var part in parts)
            {
                if (part.Count == 0)
                    throw new SyntaxError(0, 0, "Unexpected symbol ','");

                if (part[0] is (TokenType.Operator, ".."))
                    throw new SyntaxError(0, 0, "Literal is ambiguous between an array and a struct.");

                if (part[0] is (TokenType.Operator, "..."))
                {
                    expressions.Add(new(true, null, Parse(part.GetRange(1..))));
                }
                else
                {
                    var index = part.FindIndex(x => x is (TokenType.Operator, "="));

                    if (index == -1)
                        throw new SyntaxError(0, 0, "Literal is ambiguous between an array and a struct.");

                    var keyTokens = part.GetRange(..index);
                    var valueTokens = part.GetRange((index + 1)..);

                    if (keyTokens.Count == 0)
                        throw new SyntaxError(0, 0, "Missing identifier");

                    var keyExpr = Parse(keyTokens);

                    if (keyExpr is not Identifier identifier)
                        throw new SyntaxError(0, 0, "Invalid identifier");

                    if (valueTokens.Count == 0)
                        throw new SyntaxError(0, 0, "Missing value");

                    var expression = Parse(valueTokens);

                    expressions.Add(new(false, identifier.Name, expression));
                }
            }

            return new StructLiteral(expressions);
        }
    }
}