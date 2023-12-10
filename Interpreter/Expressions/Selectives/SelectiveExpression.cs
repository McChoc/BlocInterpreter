using System.Collections.Generic;
using Bloc.Memory;
using Bloc.Utils.Attributes;
using Bloc.Values.Core;
using Bloc.Values.Types;

namespace Bloc.Expressions.Selectives;

[Record]
internal sealed partial class SelectiveExpression : IExpression
{
    internal required IExpression Expression { get; init; }
    internal required List<Arm> Cases { get; init; }

    public IValue Evaluate(Call call)
    {
        var value = Expression.Evaluate(call).Value;

        foreach (var arm in Cases)
            if (arm.Matches(value, call))
                return arm.ResultExpression.Evaluate(call);

        return Void.Value;
    }
}