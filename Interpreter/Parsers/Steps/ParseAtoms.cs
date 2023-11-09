using System.Collections.Generic;
using System.Linq;
using Bloc.Expressions;
using Bloc.Expressions.Literals;
using Bloc.Expressions.Literals.ArrayElements;
using Bloc.Expressions.Literals.StructMembers;
using Bloc.Expressions.Patterns;
using Bloc.Identifiers;
using Bloc.Patterns;
using Bloc.Tokens;
using Bloc.Utils.Constants;
using Bloc.Utils.Exceptions;
using Bloc.Utils.Extensions;
using Bloc.Values.Core;

namespace Bloc.Parsers.Steps;

internal sealed class ParseAtoms : IParsingStep
{
    public IExpression Parse(List<IToken> tokens)
    {
        return tokens[0] switch
        {
            ParsedToken token => token.Expression,

            INamedIdentifierToken token => ParseIdentifier(token),
            LiteralKeywordToken token => ParseLiteralKeyword(token),
            NumberToken token => ParseNumber(token),
            StringToken token => ParseString(token),
            BracesToken token => ParseBraces(token),
            BracketsToken token => ParseBrackets(token),

            ParenthesesToken { Tokens.Count: 0 } => new TupleLiteral(new()),
            ParenthesesToken parentheses => ExpressionParser.Parse(parentheses.Tokens),

            _ => throw new SyntaxError(tokens[0].Start, tokens[0].End, "Unexpected token")
        };
    }

    private static NamedIdentifierExpression ParseIdentifier(INamedIdentifierToken token)
    {
        return new NamedIdentifierExpression(token.GetIdentifier());
    }

    private static NumberLiteral ParseNumber(NumberToken token)
    {
        return new NumberLiteral(token.Number);
    }

    private static StringLiteral ParseString(StringToken token)
    {
        var interpolations = token.Interpolations
            .Select(x => new StringLiteral.Interpolation(x.Index, ExpressionParser.Parse(x.Tokens)))
            .ToList();

        return new StringLiteral(token.BaseString, interpolations);
    }

    private static IExpression ParseLiteralKeyword(LiteralKeywordToken token)
    {
        return token.Text switch
        {
            Keyword.VOID => new VoidLiteral(),
            Keyword.NULL => new NullLiteral(),
            Keyword.FALSE => new BoolLiteral(false),
            Keyword.TRUE => new BoolLiteral(true),
            Keyword.NAN => new NumberLiteral(double.NaN),
            Keyword.INFINITY => new NumberLiteral(double.PositiveInfinity),

            Keyword.VOID_T => new TypeLiteral(ValueType.Void),
            Keyword.NULL_T => new TypeLiteral(ValueType.Null),
            Keyword.BOOL => new TypeLiteral(ValueType.Bool),
            Keyword.NUMBER => new TypeLiteral(ValueType.Number),
            Keyword.RANGE => new TypeLiteral(ValueType.Range),
            Keyword.STRING => new TypeLiteral(ValueType.String),
            Keyword.ARRAY => new TypeLiteral(ValueType.Array),
            Keyword.STRUCT => new TypeLiteral(ValueType.Struct),
            Keyword.TUPLE => new TypeLiteral(ValueType.Tuple),
            Keyword.FUNC => new TypeLiteral(ValueType.Func),
            Keyword.TASK => new TypeLiteral(ValueType.Task),
            Keyword.ITER => new TypeLiteral(ValueType.Iter),
            Keyword.REFERENCE => new TypeLiteral(ValueType.Reference),
            Keyword.EXTERN => new TypeLiteral(ValueType.Extern),
            Keyword.TYPE => new TypeLiteral(ValueType.Type),
            Keyword.PATTERN => new TypeLiteral(ValueType.Pattern),

            Keyword.ANY => new PatternLiteral(new AnyPattern()),
            Keyword.NONE => new PatternLiteral(new NonePattern()),

            _ => throw new System.Exception()
        };
    }

    private static IExpression ParseBraces(BracesToken token)
    {
        switch (token.Tokens)
        {
            case []:
                throw new SyntaxError(token.End - 1, token.End, "Unexpected symbol '}'");
            case [SymbolToken(Symbol.BIT_OR)]:
                return new ArrayLiteral(new());
            case [SymbolToken(Symbol.BIT_AND)]:
                return new StructLiteral(new());
        }

        var parts = token.Tokens.Split(x => x is SymbolToken(Symbol.COMMA));

        if (parts[^1].Count == 0)
            parts.RemoveAt(parts.Count - 1);

        return GetBracesContentType(parts) switch
        {
            BracesToken.ContentType.Array => ParseArray(parts),
            BracesToken.ContentType.Struct => ParseStruct(parts),
            _ => throw new System.Exception()
        };
    }

    private static BracesToken.ContentType GetBracesContentType (List<List<IToken>> parts)
    {
        foreach (var part in parts)
        {
            switch (part)
            {
                case []:
                    throw new SyntaxError(0, 0, $"Unexpected symbol '{Symbol.COMMA}'");
                case [INamedIdentifierToken]:
                    continue;
                case [SymbolToken(Symbol.UNPACK_STRUCT), ..]:
                case [INamedIdentifierToken, SymbolToken(Symbol.COLON), ..]:
                    return BracesToken.ContentType.Struct;
                default:
                    return BracesToken.ContentType.Array;
            }
        }

        return BracesToken.ContentType.Array;
    }

    private static ArrayLiteral ParseArray(List<List<IToken>> parts)
    {
        var elements = new List<IElement>();

        foreach (var part in parts)
        {
            IElement element = part switch
            {
                [] => throw new SyntaxError(0, 0, $"Unexpected symbol '{Symbol.COMMA}'"),
                [INamedIdentifierToken, SymbolToken(Symbol.COLON), ..] => throw new SyntaxError(0, 0, $"Unexpected symbol '{Symbol.COLON}'"),
                [SymbolToken(Symbol.UNPACK_ARRAY)] => throw new SyntaxError(0, 0, "Missing value"),
                [SymbolToken(Symbol.UNPACK_ARRAY), ..] => new UnpackedElement(ExpressionParser.Parse(part.GetRange(1..))),
                _ => new Element(ExpressionParser.Parse(part)),
            };

            elements.Add(element);
        }

        return new ArrayLiteral(elements);
    }

    private static StructLiteral ParseStruct(List<List<IToken>> parts)
    {
        var members = new List<IMember>();

        foreach (var part in parts)
        {
            IMember member = part switch
            {
                [] => throw new SyntaxError(0, 0, $"Unexpected symbol '{Symbol.COMMA}'"),
                [INamedIdentifierToken token] => new Member(token.GetIdentifier(), ExpressionParser.Parse(part)),
                [INamedIdentifierToken, SymbolToken(Symbol.COLON)] => throw new SyntaxError(0, 0, "Missing value."),
                [INamedIdentifierToken token, SymbolToken(Symbol.COLON), ..] => new Member(token.GetIdentifier(), ExpressionParser.Parse(part.GetRange(2..))),
                [SymbolToken(Symbol.UNPACK_STRUCT)] => throw new SyntaxError(0, 0, "Missing value"),
                [SymbolToken(Symbol.UNPACK_STRUCT), ..] => new UnpackedMember(ExpressionParser.Parse(part.GetRange(1..))),
                _ => throw new SyntaxError(part[0].Start, part[^1].End, "Unexpected token."),
            };

            members.Add(member);
        }

        return new StructLiteral(members);
    }

    private static IExpression ParseBrackets(BracketsToken token)
    {
        switch (token.Tokens)
        {
            case []:
                throw new SyntaxError(token.End - 1, token.End, "Unexpected symbol ']'");
            case [SymbolToken(Symbol.BIT_OR)]:
                return new ArrayPatternLiteral(new(), null, -1);
            case [SymbolToken(Symbol.BIT_AND)]:
                return new StructPatternLiteral(new(), null, false);
        }

        var parts = token.Tokens.Split(x => x is SymbolToken(Symbol.COMMA));

        if (parts[^1].Count == 0)
            parts.RemoveAt(parts.Count - 1);

        return GetBracketsContentType(parts) switch
        {
            BracketsToken.ContentType.ArrayPattern => ParseArrayPattern(parts),
            BracketsToken.ContentType.StructPattern => ParseStructPattern(parts),
            _ => throw new System.Exception()
        };
    }

    private static BracketsToken.ContentType GetBracketsContentType(List<List<IToken>> parts)
    {
        foreach (var part in parts)
        {
            switch (part)
            {
                case []:
                    throw new SyntaxError(0, 0, $"Unexpected symbol '{Symbol.COMMA}'");
                case [INamedIdentifierToken]:
                    continue;
                case [SymbolToken(Symbol.UNPACK_STRUCT), ..]:
                case [INamedIdentifierToken, SymbolToken(Symbol.QUESTION)]:
                case [INamedIdentifierToken, SymbolToken(Symbol.COLON), ..]:
                case [INamedIdentifierToken, SymbolToken(Symbol.QUESTION), SymbolToken(Symbol.COLON), ..]:
                    return BracketsToken.ContentType.StructPattern;
                default:
                    return BracketsToken.ContentType.ArrayPattern;
            }
        }

        return BracketsToken.ContentType.ArrayPattern;
    }

    private static ArrayPatternLiteral ParseArrayPattern(List<List<IToken>> parts)
    {
        int packIndex = -1;
        IExpression? packExpression = null;
        var expressions = new List<IExpression>();

        foreach (var part in parts)
        {
            switch (part)
            {
                case []:
                    throw new SyntaxError(0, 0, $"Unexpected symbol '{Symbol.COMMA}'");
                case [INamedIdentifierToken, SymbolToken(Symbol.COLON), ..]:
                    throw new SyntaxError(0, 0, $"Unexpected symbol '{Symbol.COLON}'");
                case [SymbolToken(Symbol.UNPACK_ARRAY), ..] when packIndex > -1:
                    throw new SyntaxError(0, 0, "The array unpack syntax can only be used once per array pattern.");
                case [SymbolToken(Symbol.UNPACK_ARRAY)]:
                    packIndex = expressions.Count;
                    break;
                case [SymbolToken(Symbol.UNPACK_ARRAY), ..]:
                    packIndex = expressions.Count;
                    packExpression = ExpressionParser.Parse(part.GetRange(1..));
                    break;
                default:
                    var expression = ExpressionParser.Parse(part);
                    expressions.Add(expression);
                    break;
            }
        }

        return new ArrayPatternLiteral(expressions, packExpression, packIndex);
    }

    private static StructPatternLiteral ParseStructPattern(List<List<IToken>> parts)
    {
        bool hasPack = false;
        IExpression? packExpression = null;
        var members = new List<(INamedIdentifier, IExpression, bool)>();

        foreach (var part in parts)
        {
            switch (part)
            {
                case []:
                    throw new SyntaxError(0, 0, $"Unexpected symbol '{Symbol.COMMA}'");
                case [INamedIdentifierToken token]:
                    members.Add((token.GetIdentifier(), new PatternLiteral(new AnyPattern()), false));
                    break;
                case [INamedIdentifierToken token, SymbolToken(Symbol.QUESTION)]:
                    members.Add((token.GetIdentifier(), new PatternLiteral(new AnyPattern()), true));
                    break;
                case [INamedIdentifierToken, SymbolToken(Symbol.COLON)]:
                case [INamedIdentifierToken, SymbolToken(Symbol.QUESTION), SymbolToken(Symbol.COLON)]:
                    throw new SyntaxError(0, 0, "Missing value.");
                case [INamedIdentifierToken token, SymbolToken(Symbol.COLON), ..]:
                    members.Add((token.GetIdentifier(), ExpressionParser.Parse(part.GetRange(2..)), false));
                    break;
                case [INamedIdentifierToken token, SymbolToken(Symbol.QUESTION), SymbolToken(Symbol.COLON), ..]:
                    members.Add((token.GetIdentifier(), ExpressionParser.Parse(part.GetRange(3..)), true));
                    break;
                case [SymbolToken(Symbol.UNPACK_STRUCT), ..] when hasPack:
                    throw new SyntaxError(0, 0, "The struct unpack syntax can only be used once per struct pattern.");
                case [SymbolToken(Symbol.UNPACK_STRUCT)]:
                    hasPack = true;
                    break;
                case [SymbolToken(Symbol.UNPACK_STRUCT), ..]:
                    hasPack = true;
                    packExpression = ExpressionParser.Parse(part.GetRange(1..));
                    break;
                default:
                    throw new SyntaxError(part[0].Start, part[^1].End, "Unexpected token.");
            }
        }

        return new StructPatternLiteral(members, packExpression, hasPack);
    }
}