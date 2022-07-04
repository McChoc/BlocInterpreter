using System;
using System.Collections.Generic;
using CmmInterpretor.Expressions;
using CmmInterpretor.Extensions;
using CmmInterpretor.Operators.Arithmetic;
using CmmInterpretor.Operators.Bitwise;
using CmmInterpretor.Operators.Boolean;
using CmmInterpretor.Operators.Character;
using CmmInterpretor.Operators.Collection;
using CmmInterpretor.Operators.Misc;
using CmmInterpretor.Operators.Reference;
using CmmInterpretor.Tokens;
using CmmInterpretor.Utils.Exceptions;
using Nullable = CmmInterpretor.Operators.Type.Nullable;

namespace CmmInterpretor
{
    internal static partial class ExpressionParser
    {
        private static IExpression ParseUnaries(List<Token> tokens, int precedence)
        {
            if (tokens.Count == 0)
                throw new SyntaxError(0, 0, "Missing value");

            if (tokens[0] is (TokenType.Operator or TokenType.Keyword,
                "+" or "-" or "~" or "!" or "++" or "--" or "~~" or "!!" or "len" or
                "chr" or "ord" or "val" or "ref" or "new" or "await" or "nameof" or "typeof"))
            {
                var operand = ParseUnaries(tokens.GetRange(1..), precedence);

                return tokens[0].Text switch
                {
                    "+" => new Plus(operand),
                    "-" => new Minus(operand),
                    "~" => new Complement(operand),
                    "!" => new Negation(operand),
                    "++" => new PreIncrement(operand),
                    "--" => new PreDecrement(operand),
                    "~~" => new PreComplement(operand),
                    "!!" => new PreNegation(operand),
                    "len" => new Length(operand),
                    "chr" => new Character(operand),
                    "ord" => new Ordinal(operand),
                    "val" => new Value(operand),
                    "ref" => new Reference(operand),
                    "new" => new Allocation(operand),
                    "await" => new Await(operand),
                    "nameof" => new Nameof(operand),
                    "typeof" => new Typeof(operand),
                    _ => throw new Exception()
                };
            }

            if (tokens[^1] is (TokenType.Operator, "++" or "--" or "~~" or "!!" or "?"))
            {
                var operand = ParseUnaries(tokens.GetRange(..^1), precedence);

                return tokens[^1].Text switch
                {
                    "++" => new PostIncrement(operand),
                    "--" => new PostDecrement(operand),
                    "~~" => new PostComplement(operand),
                    "!!" => new PostNegation(operand),
                    "?" => new Nullable(operand),
                    _ => throw new Exception()
                };
            }

            return Parse(tokens, precedence - 1);
        }
    }
}