using System;
using System.Collections.Generic;
using System.Linq;
using Bloc.Memory;
using Bloc.Values.Core;
using Void = Bloc.Values.Types.Void;

namespace Bloc.Expressions.Switch;

internal sealed class SwitchExpression : IExpression
{
    internal required IExpression Expression { get; init; }
    internal required List<IArm> Arms { get; init; }
    internal required IExpression? Default { get; init; }

    public IValue Evaluate(Call call)
    {
        var value = Expression.Evaluate(call).Value;

        foreach (var arm in Arms)
            if (arm.Matches(value, call))
                return arm.Expression.Evaluate(call);

        return Default?.Evaluate(call) ?? Void.Value;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Expression, Default, Arms.Count);
    }

    public override bool Equals(object other)
    {
        return other is SwitchExpression @switch &&
            Expression.Equals(@switch.Expression) &&
            Equals(Default, @switch.Default) &&
            Arms.SequenceEqual(@switch.Arms);
    }
}