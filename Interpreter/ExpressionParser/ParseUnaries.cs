using System.Collections.Generic;
using Bloc.Exceptions;
using Bloc.Expressions;
using Bloc.Extensions;
using Bloc.Operators;
using Bloc.Tokens;

namespace Bloc
{
    internal static partial class ExpressionParser
    {
        private static IExpression ParseUnaries(List<Token> tokens, int precedence)
        {
            if (tokens.Count == 0)
                throw new SyntaxError(0, 0, "Missing value");

            if (tokens[0] is (TokenType.Operator or TokenType.Keyword,
                "+" or "-" or "~" or "!" or "++" or "--" or "~~" or "!!" or "len" or "chr" or "ord" or "ref" or "val" or "val val" or
                "let" or "let new" or "new" or "const new" or "delete" or "global" or "nonlocal" or "await" or "next" or "nameof" or "typeof"))
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
                    "ref" => new Ref(operand),
                    "val" => new Val(operand),
                    "val val" => new ValVal(operand),
                    "let" => new Let(operand),
                    "let new" => new LetNew(operand),
                    "new" => new New(operand),
                    "const new" => new ConstNew(operand),
                    "delete" => new Delete(operand),
                    "global" => new Global(operand),
                    "nonlocal" => new Nonlocal(operand),
                    "await" => new Await(operand),
                    "next" => new Next(operand),
                    "nameof" => new Nameof(operand),
                    "typeof" => new Typeof(operand),
                    _ => throw new System.Exception()
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
                    _ => throw new System.Exception()
                };
            }

            return Parse(tokens, precedence - 1);
        }
    }
}