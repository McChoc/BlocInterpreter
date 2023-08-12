using System.Collections.Generic;
using System.Linq;
using Bloc.Identifiers;
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
internal sealed partial class StructPatternLiteral : IExpression
{
    private readonly bool _hasPack;
    private readonly IExpression? _packExpression;
    private readonly List<(INamedIdentifier identifier, IExpression expression)> _expressions;

    internal StructPatternLiteral(List<(INamedIdentifier, IExpression)> expressions, IExpression? packExpression, bool hasPack)
    {
        _expressions = expressions;
        _packExpression = packExpression;
        _hasPack = hasPack;
    }

    public IValue Evaluate(Call call)
    {
        var patterns = new Dictionary<string, IPatternNode>();

        foreach (var (identifier, expression) in _expressions)
        {
            var name = identifier.GetName(call);

            if (patterns.ContainsKey(name))
                throw new Throw("Duplicate keys");

            patterns[name] = GetPattern(expression, call);
        }

        var packPattern = _packExpression is not null
            ? GetPattern(_packExpression, call)
            : null;

        return new Pattern(new StructPattern(patterns, packPattern, _hasPack));
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