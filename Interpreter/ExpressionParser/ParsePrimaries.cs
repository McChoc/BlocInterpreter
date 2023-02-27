using System.Collections.Generic;
using System.Linq;
using Bloc.Exceptions;
using Bloc.Expressions;
using Bloc.Extensions;
using Bloc.Operators;
using Bloc.Scanners;
using Bloc.Tokens;
using Bloc.Utils;
using Bloc.Values;

namespace Bloc;

internal static partial class ExpressionParser
{
    private static IExpression ParsePrimaries(List<Token> tokens, int precedence)
    {
        var expression = tokens[0] switch
        {
            Literal literal => literal.Expression,

            { Type: TokenType.Braces } => ParseBlock(tokens[0]),
            { Type: TokenType.Parentheses } => Parse(TokenScanner.Scan(tokens[0]).ToList()),
            { Type: TokenType.Word or TokenType.Identifier } => new Identifier(tokens[0].Text),

            (TokenType.AccessKeyword, Keyword.RECALL) => new Recall(),

            (TokenType.LiteralKeyword, Keyword.VOID)        => new VoidLiteral(),
            (TokenType.LiteralKeyword, Keyword.NULL)        => new NullLiteral(),
            (TokenType.LiteralKeyword, Keyword.FALSE)       => new BoolLiteral(false),
            (TokenType.LiteralKeyword, Keyword.TRUE)        => new BoolLiteral(true),
            (TokenType.LiteralKeyword, Keyword.NAN)         => new NumberLiteral(double.NaN),
            (TokenType.LiteralKeyword, Keyword.INFINITY)    => new NumberLiteral(double.PositiveInfinity),

            (TokenType.LiteralKeyword, Keyword.VOID_T)      => new TypeLiteral(ValueType.Void),
            (TokenType.LiteralKeyword, Keyword.NULL_T)      => new TypeLiteral(ValueType.Null),
            (TokenType.LiteralKeyword, Keyword.BOOL)        => new TypeLiteral(ValueType.Bool),
            (TokenType.LiteralKeyword, Keyword.NUMBER)      => new TypeLiteral(ValueType.Number),
            (TokenType.LiteralKeyword, Keyword.RANGE)       => new TypeLiteral(ValueType.Range),
            (TokenType.LiteralKeyword, Keyword.STRING)      => new TypeLiteral(ValueType.String),
            (TokenType.LiteralKeyword, Keyword.ARRAY)       => new TypeLiteral(ValueType.Array),
            (TokenType.LiteralKeyword, Keyword.STRUCT)      => new TypeLiteral(ValueType.Struct),
            (TokenType.LiteralKeyword, Keyword.TUPLE)       => new TypeLiteral(ValueType.Tuple),
            (TokenType.LiteralKeyword, Keyword.FUNC)        => new TypeLiteral(ValueType.Func),
            (TokenType.LiteralKeyword, Keyword.TASK)        => new TypeLiteral(ValueType.Task),
            (TokenType.LiteralKeyword, Keyword.ITER)        => new TypeLiteral(ValueType.Iter),
            (TokenType.LiteralKeyword, Keyword.REFERENCE)   => new TypeLiteral(ValueType.Reference),
            (TokenType.LiteralKeyword, Keyword.EXTERN)      => new TypeLiteral(ValueType.Extern),
            (TokenType.LiteralKeyword, Keyword.TYPE)        => new TypeLiteral(ValueType.Type),

            _ => throw new SyntaxError(tokens[0].Start, tokens[0].End, "Unexpected symbol")
        };

        for (var i = 1; i < tokens.Count; i++)
        {
            if (tokens[i] is (TokenType.Symbol, Symbol.ACCESS_MEMBER))
            {
                if (tokens.Count <= i)
                    throw new SyntaxError(tokens[i].Start, tokens[i].End, "Missing identifier");

                if (tokens[i + 1].Type != TokenType.Identifier)
                    throw new SyntaxError(tokens[i].Start, tokens[i].End, "Missing identifier");

                expression = new MemberAccess(expression, tokens[i + 1].Text);

                i++;
            }
            else if (tokens[i].Type == TokenType.Brackets)
            {
                var index = Parse(TokenScanner.Scan(tokens[i]).ToList());

                expression = new Indexer(expression, index);
            }
            else if (tokens[i].Type == TokenType.Parentheses)
            {
                var content = TokenScanner.Scan(tokens[i]).ToList();

                var arguments = new List<Invocation.Argument>();

                if (content.Count > 0)
                {
                    foreach (var part in content.Split(x => x is (TokenType.Symbol, Symbol.COMMA)))
                    {
                        if (part.Count == 0)
                        {
                            if (arguments.Count > 0 && arguments[^1].Type is Invocation.ArgumentType.Named or Invocation.ArgumentType.UnpackedStruct)
                                throw new SyntaxError(0, 0, "All the positional arguments must apear before any named arguments");

                            arguments.Add(new(null, Invocation.ArgumentType.Positional, new VoidLiteral()));
                        }
                        else if (part[0] is (TokenType.Symbol, Symbol.UNPACK_ARRAY))
                        {
                            if (arguments.Count > 0 && arguments[^1].Type is Invocation.ArgumentType.Named or Invocation.ArgumentType.UnpackedStruct)
                                throw new SyntaxError(part[0].Start, part[^1].End, "All the positional arguments must apear before any named arguments");

                            arguments.Add(new(null, Invocation.ArgumentType.UnpackedArray, Parse(part.GetRange(1..))));
                        }
                        else if (part[0] is (TokenType.Symbol, Symbol.UNPACK_STRUCT))
                        {
                            arguments.Add(new(null, Invocation.ArgumentType.UnpackedStruct, Parse(part.GetRange(1..))));
                        }
                        else
                        {
                            var index = part.FindIndex(x => x is (TokenType.Symbol, Symbol.ASSIGN));

                            if (index == -1)
                            {
                                if (arguments.Count > 0 && arguments[^1].Type is Invocation.ArgumentType.Named or Invocation.ArgumentType.UnpackedStruct)
                                    throw new SyntaxError(part[0].Start, part[^1].End, "All the positional arguments must apear before any named arguments");

                                arguments.Add(new(null, Invocation.ArgumentType.Positional, Parse(part)));
                            }
                            else
                            {
                                var keyTokens = part.GetRange(..index);
                                var valueTokens = part.GetRange((index + 1)..);

                                if (keyTokens.Count == 0)
                                    throw new SyntaxError(0, 0, "Missing identifier");

                                var keyExpr = Parse(keyTokens);

                                if (keyExpr is not Identifier identifier)
                                    throw new SyntaxError(0, 0, "Invalid identifier");

                                if (valueTokens.Count == 0)
                                    throw new SyntaxError(0, 0, "Missing value");

                                arguments.Add(new(identifier.Name, Invocation.ArgumentType.Named, Parse(valueTokens)));
                            }
                        }
                    }
                }

                expression = new Invocation(expression, arguments);
            }
            else
            {
                throw new SyntaxError(tokens[i].Start, tokens[i].End, "Unexpected symbol");
            }
        }

        return expression;
    }
}