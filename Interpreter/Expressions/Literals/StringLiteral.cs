using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bloc.Expressions.SubExpressions;
using Bloc.Memory;
using Bloc.Values;
using String = Bloc.Values.String;

namespace Bloc.Expressions.Literals;

internal sealed class StringLiteral : IExpression
{
    private readonly string _baseString;
    private readonly List<Interpolation> _interpolations;

    internal StringLiteral(string baseString, List<Interpolation> interpolations)
    {
        _baseString = baseString;
        _interpolations = interpolations;
    }

    public IValue Evaluate(Call call)
    {
        var offset = 0;

        var builder = new StringBuilder(_baseString);

        foreach (var interpolation in _interpolations)
        {
            var value = interpolation.Expression.Evaluate(call).Value;

            var @string = String.ImplicitCast(value);

            builder.Insert(interpolation.Index + offset, @string.Value);
            offset += @string.Value.Length;
        }

        return new String(builder.ToString());
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(_baseString, _interpolations.Count);
    }

    public override bool Equals(object other)
    {
        return other is StringLiteral literal &&
            _baseString == literal._baseString &&
            _interpolations.SequenceEqual(literal._interpolations);
    }
}