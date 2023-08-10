using System.Collections.Generic;
using System.Text;
using Bloc.Memory;
using Bloc.Utils.Attributes;
using Bloc.Values.Core;
using Bloc.Values.Types;

namespace Bloc.Expressions.Literals;

[Record]
internal sealed partial class StringLiteral : IExpression
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
        int offset = 0;
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

    internal sealed record Interpolation(int Index, IExpression Expression);
}