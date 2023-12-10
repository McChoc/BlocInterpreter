using Bloc.Memory;
using Bloc.Values.Core;

namespace Bloc.Expressions.Selectives;

internal abstract record Arm(IExpression ComparedExpression, IExpression ResultExpression)
{
    public abstract bool Matches(Value value, Call call);
}