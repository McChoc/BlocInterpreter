using System;
using System.Collections.Generic;
using System.Linq;
using Bloc.Memory;
using Bloc.Patterns;
using Bloc.Results;
using Bloc.Utils.Helpers;
using Bloc.Values.Behaviors;
using Bloc.Values.Core;
using Bloc.Values.Types;

namespace Bloc.Expressions.Patterns;

internal sealed class StructPatternLiteral : IExpression
{
    private readonly bool _hasPack;
    private readonly IExpression? _packExpression;
    private readonly Dictionary<string, IExpression> _expressions;

    internal StructPatternLiteral(Dictionary<string, IExpression> expressions, IExpression? packExpression, bool hasPack)
    {
        _expressions = expressions;
        _packExpression = packExpression;
        _hasPack = hasPack;
    }

    public IValue Evaluate(Call call)
    {
        var patterns = _expressions
            .ToDictionary(x => x.Key, x => GetPattern(x.Value, call));

        var packPattern = _packExpression is not null
            ? GetPattern(_packExpression, call)
            : null;

        return new Pattern(new StructPattern(patterns, packPattern, _hasPack));
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(_hasPack, _packExpression, _expressions.Count);
    }

    public override bool Equals(object other)
    {
        if (other is not StructPatternLiteral literal)
            return false;

        if (_hasPack != literal._hasPack)
            return false;

        if (!Equals(_packExpression, literal._packExpression))
            return false;

        if (_expressions.Count != literal._expressions.Count)
            return false;

        foreach (var key in _expressions.Keys)
        {
            if (!literal._expressions.TryGetValue(key, out var value))
                return false;

            if (_expressions[key] != value)
                return false;
        }

        return true;
    }

    private static IPatternNode GetPattern(IExpression expression, Call call)
    {
        var value = expression.Evaluate(call).Value;

        value = ReferenceHelper.Resolve(value, call.Engine.Options.HopLimit).Value;

        if (value is not IPattern pattern)
            throw new Throw($"The members of a struct pattern must also be patterns");

        return pattern.GetRoot();
    }
}