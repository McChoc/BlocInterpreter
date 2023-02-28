using System.Collections.Generic;
using System.Linq;
using Bloc.Constants;
using Bloc.Exceptions;
using Bloc.Expressions;
using Bloc.Operators;
using Bloc.Tokens;
using Bloc.Utils.Extensions;
using Bloc.Values;

namespace Bloc;

internal static partial class ExpressionParser
{
    private static IExpression ParsePrimaries(List<Token> tokens, int precedence)
    {
        var expression = tokens[0] switch
        {
            IIdentifierToken identifier     => new Identifier(identifier.Text),

            GroupToken group                => Parse(group.Tokens),

            NumberToken number              => ParseNumber(number),
            StringToken @string             => ParseString(@string),
            ArrayToken array                => ParseArray(array),
            StructToken @struct             => ParseStruct(@struct),
            FuncToken func                  => func.Literal,

            LiteralToken(Keyword.VOID)      => new VoidLiteral(),
            LiteralToken(Keyword.NULL)      => new NullLiteral(),
            LiteralToken(Keyword.FALSE)     => new BoolLiteral(false),
            LiteralToken(Keyword.TRUE)      => new BoolLiteral(true),
            LiteralToken(Keyword.NAN)       => new NumberLiteral(double.NaN),
            LiteralToken(Keyword.INFINITY)  => new NumberLiteral(double.PositiveInfinity),

            LiteralToken(Keyword.VOID_T)    => new TypeLiteral(ValueType.Void),
            LiteralToken(Keyword.NULL_T)    => new TypeLiteral(ValueType.Null),
            LiteralToken(Keyword.BOOL)      => new TypeLiteral(ValueType.Bool),
            LiteralToken(Keyword.NUMBER)    => new TypeLiteral(ValueType.Number),
            LiteralToken(Keyword.RANGE)     => new TypeLiteral(ValueType.Range),
            LiteralToken(Keyword.STRING)    => new TypeLiteral(ValueType.String),
            LiteralToken(Keyword.ARRAY)     => new TypeLiteral(ValueType.Array),
            LiteralToken(Keyword.STRUCT)    => new TypeLiteral(ValueType.Struct),
            LiteralToken(Keyword.TUPLE)     => new TypeLiteral(ValueType.Tuple),
            LiteralToken(Keyword.FUNC)      => new TypeLiteral(ValueType.Func),
            LiteralToken(Keyword.TASK)      => new TypeLiteral(ValueType.Task),
            LiteralToken(Keyword.ITER)      => new TypeLiteral(ValueType.Iter),
            LiteralToken(Keyword.REFERENCE) => new TypeLiteral(ValueType.Reference),
            LiteralToken(Keyword.EXTERN)    => new TypeLiteral(ValueType.Extern),
            LiteralToken(Keyword.TYPE)      => new TypeLiteral(ValueType.Type),

            _ => throw new SyntaxError(tokens[0].Start, tokens[0].End, "Unexpected symbol")
        };

        for (var i = 1; i < tokens.Count; i++)
        {
            if (tokens[i] is SymbolToken(Symbol.ACCESS_MEMBER))
            {
                if (tokens.Count <= i)
                    throw new SyntaxError(tokens[i].Start, tokens[i].End, "Missing identifier");

                if (tokens[i + 1] is not IIdentifierToken identifier)
                    throw new SyntaxError(tokens[i].Start, tokens[i].End, "Missing identifier");

                expression = new MemberAccess(expression, identifier.Text);

                i++;
            }
            else if (tokens[i] is IndexerToken indexer)
            {
                var index = Parse(indexer.Tokens);

                expression = new Indexer(expression, index);
            }
            else if (tokens[i] is GroupToken group)
            {
                var content = group.Tokens;

                var arguments = new List<Invocation.Argument>();

                if (content.Count > 0)
                {
                    foreach (var part in content.Split(x => x is SymbolToken(Symbol.COMMA)))
                    {
                        if (part.Count == 0)
                        {
                            if (arguments.Count > 0 && arguments[^1].Type is Invocation.ArgumentType.Named or Invocation.ArgumentType.UnpackedStruct)
                                throw new SyntaxError(0, 0, "All the positional arguments must apear before any named arguments");

                            arguments.Add(new(null, Invocation.ArgumentType.Positional, new VoidLiteral()));
                        }
                        else if (part[0] is SymbolToken(Symbol.UNPACK_ARRAY))
                        {
                            if (arguments.Count > 0 && arguments[^1].Type is Invocation.ArgumentType.Named or Invocation.ArgumentType.UnpackedStruct)
                                throw new SyntaxError(part[0].Start, part[^1].End, "All the positional arguments must apear before any named arguments");

                            arguments.Add(new(null, Invocation.ArgumentType.UnpackedArray, Parse(part.GetRange(1..))));
                        }
                        else if (part[0] is SymbolToken(Symbol.UNPACK_STRUCT))
                        {
                            arguments.Add(new(null, Invocation.ArgumentType.UnpackedStruct, Parse(part.GetRange(1..))));
                        }
                        else
                        {
                            var index = part.FindIndex(x => x is SymbolToken(Symbol.ASSIGN));

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

    private static NumberLiteral ParseNumber(NumberToken token)
    {
        return new NumberLiteral(token.Number);
    }

    private static StringLiteral ParseString(StringToken token)
    {
        var interpolations = token.Interpolations
            .Select(x => new StringLiteral.Interpolation(x.Index, Parse(x.Tokens)))
            .ToList();

        return new StringLiteral(token.BaseString, interpolations);
    }

    private static ArrayLiteral ParseArray(ArrayToken token)
    {
        var parts = token.Tokens.Split(x => x is SymbolToken(Symbol.COMMA));

        var expressions = new List<ArrayLiteral.SubExpression>();

        foreach (var part in parts)
        {
            if (part.Count == 0)
                throw new SyntaxError(0, 0, $"Unexpected symbol '{Symbol.COMMA}'");

            if (part.Any(x => x is SymbolToken(Symbol.ASSIGN)))
                throw new SyntaxError(0, 0, "Literal is ambiguous between an array and a struct.");

            if (part[0] is SymbolToken(Symbol.UNPACK_STRUCT))
                throw new SyntaxError(0, 0, "Literal is ambiguous between an array and a struct.");

            if (part[0] is SymbolToken(Symbol.UNPACK_ARRAY))
                expressions.Add(new(true, Parse(part.GetRange(1..))));
            else
                expressions.Add(new(false, Parse(part)));
        }

        return new ArrayLiteral(expressions);
    }

    private static StructLiteral ParseStruct(StructToken token)
    {
        var parts = token.Tokens.Split(x => x is SymbolToken(Symbol.COMMA));

        var expressions = new List<StructLiteral.SubExpression>();

        foreach (var part in parts)
        {
            if (part.Count == 0)
                throw new SyntaxError(0, 0, $"Unexpected symbol '{Symbol.COMMA}'");

            if (part[0] is SymbolToken(Symbol.UNPACK_ARRAY))
                throw new SyntaxError(0, 0, "Literal is ambiguous between an array and a struct.");

            if (part[0] is SymbolToken(Symbol.UNPACK_STRUCT))
            {
                expressions.Add(new(true, null, Parse(part.GetRange(1..))));
            }
            else
            {
                var index = part.FindIndex(x => x is SymbolToken(Symbol.ASSIGN));

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