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
    private readonly List<(INamedIdentifier Identifier, IExpression Expression, bool Optional)> _members;

    internal StructPatternLiteral(List<(INamedIdentifier, IExpression, bool)> expressions, IExpression? packExpression, bool hasPack)
    {
        _members = expressions;
        _packExpression = packExpression;
        _hasPack = hasPack;
    }

    public IValue Evaluate(Call call)
    {
        var members = new Dictionary<string, (IPatternNode, bool)>();

        foreach (var (identifier, expression, optional) in _members)
        {
            var name = identifier.GetName(call);

            if (members.ContainsKey(name))
                throw new Throw("Duplicate keys");

            members[name] = (GetPattern(expression, call), optional);
        }

        var packPattern = _packExpression is not null
            ? GetPattern(_packExpression, call)
            : null;

        return new Pattern(new StructPattern(members, packPattern, _hasPack));
    }

    private static IPatternNode GetPattern(IExpression expression, Call call)
    {
        var value = expression.Evaluate(call).Value;

        value = ReferenceHelper.Resolve(value, call.Engine.Options.HopLimit).Value;

        if (value is not IPattern pattern)
            throw new Throw($"The members of a struct pattern must be patterns");

        return pattern.GetRoot();
    }
}