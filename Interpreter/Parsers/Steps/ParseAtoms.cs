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

internal sealed class ParseAtoms : ParsingStep
{
    public ParseAtoms(ParsingStep? nextStep)
        : base(nextStep) { }

    internal override IExpression Parse(List<Token> tokens)
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
        return token.Tokens switch
        {
            [] => throw new SyntaxError(token.End - 1, token.End, "Unexpected symbol '}'"),
            [SymbolToken(Symbol.BIT_OR)] => new ArrayLiteral(new()),
            [SymbolToken(Symbol.BIT_AND)] => new StructLiteral(new()),
            var tokens when IsArray(tokens) => ParseArray(tokens),
            var tokens => ParseStruct(tokens)
        };
    }

    private static bool IsArray(List<Token> tokens)
    {
        if (tokens[0] is SymbolToken(Symbol.UNPACK_ARRAY))
            return true;

        if (tokens[0] is SymbolToken(Symbol.UNPACK_STRUCT))
            return false;

        foreach (var token in tokens)
        {
            if (token is SymbolToken(Symbol.COMMA))
                return true;

            if (token is SymbolToken(Symbol.ASSIGN))
                return false;
        }

        return true;
    }

    private static ArrayLiteral ParseArray(List<Token> tokens)
    {
        var parts = tokens.Split(x => x is SymbolToken(Symbol.COMMA));

        if (parts[^1].Count == 0)
            parts.RemoveAt(parts.Count - 1);

        var elements = new List<IElement>();

        foreach (var part in parts)
        {
            if (part.Count == 0)
                throw new SyntaxError(0, 0, $"Unexpected symbol '{Symbol.COMMA}'");

            if (part.Any(x => x is SymbolToken(Symbol.ASSIGN)))
                throw new SyntaxError(0, 0, "Literal is ambiguous between an array and a struct.");

            switch(part[0])
            {
                case SymbolToken(Symbol.UNPACK_STRUCT):
                    throw new SyntaxError(0, 0, "Literal is ambiguous between an array and a struct.");

                case SymbolToken(Symbol.UNPACK_ARRAY):
                {
                    if (part.Count <= 1)
                        throw new SyntaxError(0, 0, "Missing value");

                    var expression = ExpressionParser.Parse(part.GetRange(1..));
                    var element = new UnpackedElement(expression);
                    elements.Add(element);
                    break;
                }

                default:
                {
                    var expression = ExpressionParser.Parse(part);
                    var element = new Element(expression);
                    elements.Add(element);
                    break;
                }
            }
        }

        return new ArrayLiteral(elements);
    }

    private static StructLiteral ParseStruct(List<Token> tokens)
    {
        var parts = tokens.Split(x => x is SymbolToken(Symbol.COMMA));

        if (parts[^1].Count == 0)
            parts.RemoveAt(parts.Count - 1);

        var members = new List<IMember>();

        foreach (var part in parts)
        {
            if (part.Count == 0)
                throw new SyntaxError(0, 0, $"Unexpected symbol '{Symbol.COMMA}'");

            switch (part[0])
            {
                case INamedIdentifierToken token:
                {
                    if (!part.Any(x => x is SymbolToken(Symbol.ASSIGN)))
                        throw new SyntaxError(0, 0, "Literal is ambiguous between an array and a struct.");

                    if (part[1] is not SymbolToken(Symbol.ASSIGN))
                        throw new SyntaxError(part[1].Start, part[1].End, "Unexpected token.");

                    if (part.Count <= 2)
                        throw new SyntaxError(0, 0, "Missing value.");

                    var identifier = token.GetIdentifier();
                    var expression = ExpressionParser.Parse(part.GetRange(2..));
                    var member = new Member(identifier, expression);

                    members.Add(member);
                    break;
                }

                case SymbolToken(Symbol.UNPACK_STRUCT):
                {
                    if (part.Count <= 1)
                        throw new SyntaxError(0, 0, "Missing value");

                    var expression = ExpressionParser.Parse(part.GetRange(1..));
                    var member = new UnpackedMember(expression);
                    members.Add(member);
                    break;
                }

                default:
                    if (!part.Any(x => x is SymbolToken(Symbol.ASSIGN)))
                        throw new SyntaxError(0, 0, "Literal is ambiguous between an array and a struct.");
                    else
                        throw new SyntaxError(part[0].Start, part[^1].End, "Unexpected token.");
            }
        }

        return new StructLiteral(members);
    }

    private static IExpression ParseBrackets(BracketsToken token)
    {
        return token.Tokens switch
        {
            [] => new PatternLiteral(new EmptyPattern()),
            [SymbolToken(Symbol.BIT_OR)] => new ArrayPatternLiteral(new(), null, -1),
            [SymbolToken(Symbol.BIT_AND)] => new StructPatternLiteral(new(), null, false),
            var tokens when IsArrayPattern(tokens) => ParseArrayPattern(tokens),
            var tokens => ParseStructPattern(tokens)
        };
    }

    private static bool IsArrayPattern(List<Token> tokens)
    {
        if (tokens[0] is SymbolToken(Symbol.UNPACK_ARRAY))
            return true;

        if (tokens[0] is SymbolToken(Symbol.UNPACK_STRUCT))
            return false;

        foreach (var token in tokens)
        {
            if (token is SymbolToken(Symbol.COMMA))
                return true;

            if (token is SymbolToken(Symbol.COLON))
                return false;
        }

        return true;
    }

    private static ArrayPatternLiteral ParseArrayPattern(List<Token> tokens)
    {
        var parts = tokens.Split(x => x is SymbolToken(Symbol.COMMA));

        if (parts[^1].Count == 0)
            parts.RemoveAt(parts.Count - 1);

        int packIndex = -1;
        IExpression? packExpression = null;
        var expressions = new List<IExpression>();

        foreach (var part in parts)
        {
            if (part.Count == 0)
                throw new SyntaxError(0, 0, $"Unexpected symbol '{Symbol.COMMA}'");

            if (part.Any(x => x is SymbolToken(Symbol.COLON)))
                throw new SyntaxError(0, 0, "Literal is ambiguous between an array pattern and a struct pattern.");

            switch (part[0])
            {
                case SymbolToken(Symbol.UNPACK_STRUCT):
                    throw new SyntaxError(0, 0, "Literal is ambiguous between an array pattern and a struct pattern.");

                case SymbolToken(Symbol.UNPACK_ARRAY):
                    if (packIndex > -1)
                        throw new SyntaxError(0, 0, "The array unpack syntax can only be used once per array pattern.");

                    packIndex = expressions.Count;

                    if (part.Count <= 1)
                        continue;

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

    private static StructPatternLiteral ParseStructPattern(List<Token> tokens)
    {
        var parts = tokens.Split(x => x is SymbolToken(Symbol.COMMA));

        if (parts[^1].Count == 0)
            parts.RemoveAt(parts.Count - 1);

        bool hasPack = false;
        IExpression? packExpression = null;
        var expressions = new List<(INamedIdentifier, IExpression)>();

        foreach (var part in parts)
        {
            if (part.Count == 0)
                throw new SyntaxError(0, 0, $"Unexpected symbol '{Symbol.COMMA}'");

            switch (part[0])
            {
                case SymbolToken(Symbol.UNPACK_STRUCT):
                    if (hasPack)
                        throw new SyntaxError(0, 0, "The struct unpack syntax can only be used once per struct pattern.");

                    hasPack = true;

                    if (part.Count <= 1)
                        continue;

                    packExpression = ExpressionParser.Parse(part.GetRange(1..));
                    break;

                case INamedIdentifierToken token:
                    if (!part.Any(x => x is SymbolToken(Symbol.COLON)))
                        throw new SyntaxError(0, 0, "Literal is ambiguous between an array pattern and a struct pattern.");

                    if (part[1] is not SymbolToken(Symbol.COLON))
                        throw new SyntaxError(part[1].Start, part[1].End, "Unexpected token.");

                    if (part.Count <= 2)
                        throw new SyntaxError(0, 0, "Missing value.");


                    var identifier = token.GetIdentifier();
                    var expression = ExpressionParser.Parse(part.GetRange(2..));

                    expressions.Add((identifier, expression));
                    break;

                default:
                    if (!part.Any(x => x is SymbolToken(Symbol.COLON)))
                        throw new SyntaxError(0, 0, "Literal is ambiguous between an array pattern and a struct pattern.");
                    else
                        throw new SyntaxError(part[0].Start, part[^1].End, "Unexpected token.");
            }
        }

        return new StructPatternLiteral(expressions, packExpression, hasPack);
    }
}