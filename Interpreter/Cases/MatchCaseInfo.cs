using Bloc.Memory;
using Bloc.Results;
using Bloc.Statements;
using Bloc.Utils.Helpers;
using Bloc.Values.Behaviors;
using Bloc.Values.Core;

namespace Bloc.Cases;

internal sealed record MatchCaseInfo(Value Value, Statement Statement)
    : CaseInfo(Value, Statement)
{
    public override bool Matches(Value value, Call call)
    {
        var comparedValue = ReferenceHelper.Resolve(Value, call.Engine.Options.HopLimit).Value;

        if (comparedValue is not IPattern pattern)
            throw new Throw($"The expression of a match case must be a pattern");

        return pattern.GetRoot().Matches(value, call);
    }
}