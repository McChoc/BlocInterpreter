using System.Collections.Generic;
using Bloc.Memory;
using Bloc.Utils.Attributes;
using Bloc.Values.Core;
using Bloc.Values.Types;

namespace Bloc.Expressions.Switch;

[Record]
internal sealed partial class SwitchExpression : IExpression
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
}