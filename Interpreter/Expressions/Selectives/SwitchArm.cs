using Bloc.Memory;
using Bloc.Values.Core;

namespace Bloc.Expressions.Selectives;

internal sealed record SwitchArm(IExpression ComparedExpression, IExpression ResultExpression)
    : Arm(ComparedExpression, ResultExpression)
{
    public override bool Matches(Value value, Call call)
    {
        return ComparedExpression.Evaluate(call).Value.Equals(value);
    }
}