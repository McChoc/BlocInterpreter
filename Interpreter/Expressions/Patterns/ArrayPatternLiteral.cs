﻿using System.Collections.Generic;
using System.Linq;
using Bloc.Memory;
using Bloc.Patterns;
using Bloc.Results;
using Bloc.Utils.Attributes;
using Bloc.Utils.Helpers;
using Bloc.Values.Behaviors;
using Bloc.Values.Core;
using Bloc.Values.Types;

namespace Bloc.Expressions.Patterns;

[Record]
internal sealed partial class ArrayPatternLiteral : IExpression
{
    private readonly int _packIndex;
    private readonly IExpression? _packExpression;
    private readonly List<IExpression> _expressions;

    internal ArrayPatternLiteral(List<IExpression> expressions, IExpression? packExpression, int packIndex)
    {
        _expressions = expressions;
        _packExpression = packExpression;
        _packIndex = packIndex;
    }

    public IValue Evaluate(Call call)
    {
        var patterns = _expressions
            .Select(x => GetPattern(x, call))
            .ToList();

        var packPattern = _packExpression is not null
            ? GetPattern(_packExpression, call)
            : null;

        return new Pattern(new ArrayPattern(patterns, packPattern, _packIndex));
    }

    private static IPatternNode GetPattern(IExpression expression, Call call)
    {
        var value = expression.Evaluate(call).Value;

        value = ReferenceHelper.Resolve(value, call.Engine.Options.HopLimit).Value;

        if (value is not IPattern pattern)
            throw new Throw($"The elements of an array pattern must also be patterns");

        return pattern.GetRoot();
    }
}