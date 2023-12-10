using Bloc.Memory;
using Bloc.Results;
using Bloc.Utils.Helpers;
using Bloc.Values.Behaviors;
using Bloc.Values.Core;

namespace Bloc.Expressions.Selectives;

internal sealed record MatchArm(IExpression ComparedExpression, IExpression ResultExpression)
    : Arm(ComparedExpression, ResultExpression)
{
    public override bool Matches(Value value, Call call)
    {
        var comparedValue = ComparedExpression.Evaluate(call).Value;
        comparedValue = ReferenceHelper.Resolve(comparedValue, call.Engine.Options.HopLimit).Value;

        if (comparedValue is not IPattern pattern)
            throw new Throw($"The expression of a match arm must be a pattern");

        return pattern.GetRoot().Matches(value, call);
    }
}