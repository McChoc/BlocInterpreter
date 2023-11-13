using System.Collections.Generic;
using Bloc.Memory;
using Bloc.Patterns;
using Bloc.Results;
using Bloc.Utils.Attributes;
using Bloc.Utils.Helpers;
using Bloc.Values.Behaviors;
using Bloc.Values.Core;
using Bloc.Values.Types;

namespace Bloc.Expressions.Switch;

[Record]
internal sealed partial class SwitchExpression : IExpression
{
    internal required IExpression Expression { get; init; }
    internal required List<Case> Cases { get; init; }

    public IValue Evaluate(Call call)
    {
        var value = Expression.Evaluate(call).Value;

        foreach (var arm in Cases)
            if (arm.Matches(value, call))
                return arm.ResultExpression.Evaluate(call);

        return Void.Value;
    }

    internal sealed record Case(IExpression ComparedExpression, IExpression ResultExpression)
    {
        public bool Matches(Value value, Call call)
        {
            var pattern = GetPattern(ComparedExpression, call);

            return pattern.Matches(value, call);
        }

        private static IPatternNode GetPattern(IExpression expression, Call call)
        {
            var value = expression.Evaluate(call).Value;
            value = ReferenceHelper.Resolve(value, call.Engine.Options.HopLimit).Value;

            if (value is not IPattern pattern)
                throw new Throw($"The expression of a case arm must be a pattern");

            return pattern.GetRoot();
        }
    }
}